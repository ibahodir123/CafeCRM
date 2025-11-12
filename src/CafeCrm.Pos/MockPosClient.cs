using System;
using System.Collections.Generic;
using System.Threading.Channels;
using CafeCrm.Application.Contracts;
using CafeCrm.Domain.Entities;
using CafeCrm.Pos.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CafeCrm.Pos;

public class MockPosClient : IPosClient
{
    private static readonly string[] Tables = { "A1", "A2", "B1", "B2", "VIP1", "VIP2" };

    private readonly PosAdapterOptions _options;
    private readonly ILogger<MockPosClient> _logger;
    private readonly Channel<PosTicket> _channel = Channel.CreateUnbounded<PosTicket>();
    private CancellationTokenSource? _producerCts;
    private Task? _producerTask;
    private readonly Random _random = new();

    public MockPosClient(IOptions<PosAdapterOptions> options, ILogger<MockPosClient> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    public async Task ConnectAsync(CancellationToken cancellationToken = default)
    {
        if (!_options.Enabled)
        {
            _logger.LogInformation("POS адаптер отключен настройками.");
            _channel.Writer.TryComplete();
            return;
        }

        _producerCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        _producerTask = Task.Run(() => ProduceTicketsAsync(_producerCts.Token), cancellationToken);
        _logger.LogInformation("Mock POS client started.");
        await Task.CompletedTask;
    }

    public async Task DisconnectAsync(CancellationToken cancellationToken = default)
    {
        if (_producerCts is null)
        {
            _channel.Writer.TryComplete();
            return;
        }

        _producerCts.Cancel();
        if (_producerTask is not null)
        {
            try
            {
                await _producerTask;
            }
            catch (OperationCanceledException)
            {
                // expected on shutdown
            }
        }

        _channel.Writer.TryComplete();
    }

    public IAsyncEnumerable<PosTicket> ListenTicketsAsync(CancellationToken cancellationToken = default)
        => _channel.Reader.ReadAllAsync(cancellationToken);

    private async Task ProduceTicketsAsync(CancellationToken cancellationToken)
    {
        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                await Task.Delay(TimeSpan.FromSeconds(_options.PollingIntervalSeconds), cancellationToken);

                var ticket = new PosTicket
                {
                    TicketId = Guid.NewGuid().ToString("N"),
                    Total = Math.Round((decimal)_random.NextDouble() * 5000m, 2),
                    Cashier = "Demo касса",
                    PaymentMethod = "Card",
                    TableNumber = Tables[_random.Next(Tables.Length)],
                    ClosedAt = DateTime.UtcNow,
                    Items = new Dictionary<string, decimal>
                    {
                        { "Латте", 320 },
                        { "Десерт", 450 }
                    }
                };

                await _channel.Writer.WriteAsync(ticket, cancellationToken);
            }
        }
        catch (OperationCanceledException)
        {
            // normal shutdown
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка генерации тестового чека.");
        }
        finally
        {
            _channel.Writer.TryComplete();
        }
    }
}
