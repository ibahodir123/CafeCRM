using System;
using System.Threading;
using System.Threading.Tasks;
using CafeCrm.Application.Abstractions.Notifications;
using CafeCrm.Application.Contracts;
using CafeCrm.Domain.Entities;
using CafeCrm.Domain.Enums;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace CafeCrm.Application.Services;

public class PosTicketIngestionService : BackgroundService
{
    private readonly IPosClient _posClient;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly INotificationService _notificationService;
    private readonly ILogger<PosTicketIngestionService> _logger;

    public PosTicketIngestionService(
        IPosClient posClient,
        IServiceScopeFactory scopeFactory,
        INotificationService notificationService,
        ILogger<PosTicketIngestionService> logger)
    {
        _posClient = posClient;
        _scopeFactory = scopeFactory;
        _notificationService = notificationService;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            await _posClient.ConnectAsync(stoppingToken);
            await foreach (var ticket in _posClient.ListenTicketsAsync(stoppingToken))
            {
                await HandleTicketAsync(ticket, stoppingToken);
            }
        }
        catch (OperationCanceledException)
        {
            // graceful shutdown
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "POS ticket feed processing error.");
            _notificationService.ShowError("Lost connection with the POS adapter.");
        }
        finally
        {
            await _posClient.DisconnectAsync(CancellationToken.None);
        }
    }

    private async Task HandleTicketAsync(PosTicket ticket, CancellationToken cancellationToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var visitService = scope.ServiceProvider.GetRequiredService<VisitService>();
        var tableService = scope.ServiceProvider.GetRequiredService<ITableService>();
        var historyService = scope.ServiceProvider.GetRequiredService<IEventHistoryService>();

        var tableNumber = string.IsNullOrWhiteSpace(ticket.TableNumber) ? "POS" : ticket.TableNumber!;
        var existingVisit = await tableService.GetActiveVisitByTableAsync(tableNumber, cancellationToken);

        if (existingVisit is not null)
        {
            await tableService.AssignPosCheckToVisitAsync(
                existingVisit.Id,
                ticket.TicketId,
                ticket.Total,
                ticket.ClosedAt ?? DateTime.UtcNow,
                cancellationToken);

            await historyService.LogAsync(
                "POS_CHECK_ASSIGNED",
                $"POS check attached to table {tableNumber}",
                $"CheckId: {ticket.TicketId}, VisitId: {existingVisit.Id}",
                cancellationToken);

            _logger.LogInformation("Ticket {TicketId} linked to visit {VisitId}", ticket.TicketId, existingVisit.Id);
            _notificationService.ShowSuccess($"POS check linked to table {tableNumber}");
            return;
        }

        var created = await visitService.RegisterAsync(
            customerId: null,
            checkAmount: ticket.Total,
            tableNumber: tableNumber,
            source: VisitSource.PosTicket,
            posTicketId: ticket.TicketId,
            isTakeaway: false,
            notes: null,
            cancellationToken);

        await historyService.LogAsync(
            "POS_VISIT_CREATED",
            $"Visit created from POS check for table {tableNumber}",
            $"CheckId: {ticket.TicketId}, VisitId: {created.Id}",
            cancellationToken);

        _logger.LogInformation("Created visit {VisitId} for ticket {TicketId}", created.Id, ticket.TicketId);
        _notificationService.ShowInfo($"New visit opened for table {tableNumber}");
    }
}
