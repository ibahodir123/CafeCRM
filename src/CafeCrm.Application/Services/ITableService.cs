using System;
using System.Threading;
using System.Threading.Tasks;
using CafeCrm.Application.DTOs;

namespace CafeCrm.Application.Services;

public interface ITableService
{
    Task<IReadOnlyList<TableStatusDto>> GetActiveTablesAsync(CancellationToken cancellationToken = default);
    Task<VisitDto?> GetActiveVisitByTableAsync(string tableNumber, CancellationToken cancellationToken = default);
    Task AssignPosCheckToVisitAsync(Guid visitId, string posTicketId, decimal totalAmount, DateTime closedAt, CancellationToken cancellationToken = default);
    Task<VisitDetailDto?> GetVisitDetailAsync(Guid visitId, CancellationToken cancellationToken = default);
}
