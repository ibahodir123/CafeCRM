using System;
using System.Linq;
using CafeCrm.Application.DTOs;
using CafeCrm.Domain.Entities;
using CafeCrm.Domain.Enums;
using CafeCrm.Domain.Repositories;
using Microsoft.Extensions.Logging;

namespace CafeCrm.Application.Services;

public class VisitService
{
    private readonly IVisitRepository _visits;
    private readonly LoyaltyService _loyaltyService;
    private readonly IEventHistoryService _eventHistory;
    private readonly ILogger<VisitService> _logger;

    public VisitService(
        IVisitRepository visits,
        LoyaltyService loyaltyService,
        IEventHistoryService eventHistory,
        ILogger<VisitService> logger)
    {
        _visits = visits;
        _loyaltyService = loyaltyService;
        _eventHistory = eventHistory;
        _logger = logger;
    }

    public async Task<IReadOnlyList<VisitDto>> GetRecentAsync(int take = 10, CancellationToken cancellationToken = default)
    {
        var items = await _visits.GetRecentAsync(take, cancellationToken);
        return items.Select(MapToDto).ToList();
    }

    public async Task<VisitDto> RegisterAsync(
        Guid? customerId,
        decimal checkAmount,
        string tableNumber,
        VisitSource source,
        string? posTicketId,
        bool isTakeaway = false,
        string? notes = null,
        CancellationToken cancellationToken = default)
    {
        var visit = new Visit(customerId, tableNumber, notes, isTakeaway, source);
        if (!string.IsNullOrWhiteSpace(posTicketId))
        {
            visit.AssignPosCheck(posTicketId);
        }

        if (checkAmount > 0)
        {
            visit.ClosePosCheck(checkAmount, DateTime.UtcNow);
        }

        var saved = await _visits.AddAsync(visit, cancellationToken);
        _logger.LogInformation("Registered visit {VisitId}", saved.Id);
        return MapToDto(saved);
    }

    public async Task<VisitDto> CloseAsync(Guid visitId, decimal? finalAmount = null, CancellationToken cancellationToken = default)
    {
        var visit = await _visits.GetByIdAsync(visitId, cancellationToken)
            ?? throw new InvalidOperationException($"Visit {visitId} was not found.");

        if (visit.EndedAt.HasValue)
        {
            _logger.LogInformation("Visit {VisitId} already closed at {EndedAt}", visitId, visit.EndedAt);
            return MapToDto(visit);
        }

        var resolvedAmount = finalAmount
            ?? (visit.TotalAmount > 0 ? visit.TotalAmount : visit.OrderItems.Sum(o => o.Total));

        visit.Complete(resolvedAmount);

        await _visits.UpdateAsync(visit, cancellationToken);

        if (visit.CustomerId.HasValue && visit.TotalAmount > 0)
        {
            await _loyaltyService.AccrueAsync(visit.CustomerId.Value, visit.TotalAmount, cancellationToken);
        }

        await _eventHistory.LogAsync(
            "VISIT_CLOSED",
            $"Visit for table {visit.TableNumber} closed",
            $"VisitId: {visit.Id}; Amount: {visit.TotalAmount}",
            cancellationToken);

        _logger.LogInformation("Closed visit {VisitId} with amount {Amount}", visit.Id, visit.TotalAmount);

        return MapToDto(visit);
    }

    private static VisitDto MapToDto(Visit visit)
    {
        return new VisitDto(
            visit.Id,
            visit.StartedAt,
            visit.TotalAmount,
            visit.Customer?.Name,
            visit.Source,
            visit.TableNumber,
            visit.Notes,
            visit.IsTakeaway,
            visit.PosCheckId);
    }
}
