using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CafeCrm.Application.DTOs;
using CafeCrm.Domain.Repositories;

namespace CafeCrm.Application.Services;

public class TableService : ITableService
{
    private readonly IVisitRepository _visitRepository;

    public TableService(IVisitRepository visitRepository)
    {
        _visitRepository = visitRepository;
    }

    public async Task<IReadOnlyList<TableStatusDto>> GetActiveTablesAsync(CancellationToken cancellationToken = default)
    {
        var visits = await _visitRepository.GetActiveVisitsAsync(cancellationToken);
        return visits
            .Where(v => !string.IsNullOrWhiteSpace(v.TableNumber))
            .Select(v => new TableStatusDto
            {
                TableNumber = v.TableNumber,
                CustomerName = v.Customer?.Name ?? "Unknown guest",
                CustomerPhone = v.Customer?.Phone,
                StartedAt = v.StartedAt,
                IsTakeaway = v.IsTakeaway,
                HasPosCheck = !string.IsNullOrWhiteSpace(v.PosCheckId),
                CurrentAmount = v.OrderItems.Sum(oi => oi.Total),
                ItemCount = v.OrderItems.Count,
                VisitId = v.Id
            })
            .ToList();
    }

    public async Task<VisitDto?> GetActiveVisitByTableAsync(string tableNumber, CancellationToken cancellationToken = default)
    {
        var visit = await _visitRepository.GetActiveVisitByTableAsync(tableNumber, cancellationToken);
        if (visit is null)
        {
            return null;
        }

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

    public async Task AssignPosCheckToVisitAsync(Guid visitId, string posCheckId, decimal totalAmount, DateTime closedAt, CancellationToken cancellationToken = default)
    {
        var visit = await _visitRepository.GetByIdAsync(visitId, cancellationToken);
        if (visit is null)
        {
            return;
        }

        visit.AssignPosCheck(posCheckId);
        visit.ClosePosCheck(totalAmount, closedAt);

        await _visitRepository.UpdateAsync(visit, cancellationToken);
    }

    public async Task<VisitDetailDto?> GetVisitDetailAsync(Guid visitId, CancellationToken cancellationToken = default)
    {
        var visit = await _visitRepository.GetByIdWithCustomerAsync(visitId, cancellationToken);
        if (visit is null)
        {
            return null;
        }

        var items = visit.OrderItems
            .Select(oi => new OrderItemDto
            {
                Name = oi.Name,
                Quantity = oi.Quantity,
                Price = oi.Price,
                Total = oi.Total
            })
            .ToArray();

        return new VisitDetailDto
        {
            Id = visit.Id,
            CustomerName = visit.Customer?.Name ?? "Unknown guest",
            CustomerPhone = visit.Customer?.Phone,
            TableNumber = visit.TableNumber,
            StartedAt = visit.StartedAt,
            Notes = visit.Notes,
            IsTakeaway = visit.IsTakeaway,
            PosCheckId = visit.PosCheckId,
            OrderItems = items,
            TotalAmount = visit.TotalAmount > 0 ? visit.TotalAmount : items.Sum(i => i.Total),
            Duration = DateTime.UtcNow - visit.StartedAt
        };
    }
}



