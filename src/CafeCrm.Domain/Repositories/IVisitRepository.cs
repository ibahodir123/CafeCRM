using CafeCrm.Domain.Entities;

namespace CafeCrm.Domain.Repositories;

public interface IVisitRepository
{
    Task<IReadOnlyList<Visit>> GetRecentAsync(int take, CancellationToken cancellationToken = default);
    Task<Visit> AddAsync(Visit visit, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Visit>> GetActiveVisitsAsync(CancellationToken cancellationToken = default);
    Task<Visit?> GetActiveVisitByTableAsync(string tableNumber, CancellationToken cancellationToken = default);
    Task<Visit?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Visit?> GetByIdWithCustomerAsync(Guid id, CancellationToken cancellationToken = default);
    Task UpdateAsync(Visit visit, CancellationToken cancellationToken = default);
}
