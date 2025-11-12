using System.Linq;
using CafeCrm.Domain.Entities;
using CafeCrm.Domain.Repositories;
using CafeCrm.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CafeCrm.Infrastructure.Repositories;

internal sealed class VisitRepository : IVisitRepository
{
    private readonly CafeCrmDbContext _dbContext;

    public VisitRepository(CafeCrmDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Visit> AddAsync(Visit visit, CancellationToken cancellationToken = default)
    {
        _dbContext.Visits.Add(visit);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return visit;
    }

    public async Task<IReadOnlyList<Visit>> GetRecentAsync(int take, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Visits
            .AsNoTracking()
            .Include(v => v.Customer)
            .Include(v => v.OrderItems)
            .OrderByDescending(v => v.StartedAt)
            .Take(take)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Visit>> GetActiveVisitsAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.Visits
            .AsNoTracking()
            .Include(v => v.Customer)
            .Include(v => v.OrderItems)
            .Where(v => v.EndedAt == null)
            .OrderByDescending(v => v.StartedAt)
            .ToListAsync(cancellationToken);
    }

    public Task<Visit?> GetActiveVisitByTableAsync(string tableNumber, CancellationToken cancellationToken = default)
    {
        return _dbContext.Visits
            .Include(v => v.Customer)
            .Include(v => v.OrderItems)
            .FirstOrDefaultAsync(v => v.EndedAt == null && v.TableNumber == tableNumber, cancellationToken);
    }

    public Task<Visit?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return _dbContext.Visits
            .Include(v => v.Customer)
            .Include(v => v.OrderItems)
            .FirstOrDefaultAsync(v => v.Id == id, cancellationToken);
    }

    public Task<Visit?> GetByIdWithCustomerAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return _dbContext.Visits
            .Include(v => v.Customer)
            .Include(v => v.OrderItems)
            .FirstOrDefaultAsync(v => v.Id == id, cancellationToken);
    }

    public async Task UpdateAsync(Visit visit, CancellationToken cancellationToken = default)
    {
        _dbContext.Visits.Update(visit);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
