using System.Linq;
using CafeCrm.Domain.Entities;
using CafeCrm.Domain.Repositories;
using CafeCrm.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CafeCrm.Infrastructure.Repositories;

internal sealed class EventLogRepository : IEventLogRepository
{
    private readonly CafeCrmDbContext _dbContext;

    public EventLogRepository(CafeCrmDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(EventLog log, CancellationToken cancellationToken = default)
    {
        await _dbContext.EventLogs.AddAsync(log, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<EventLog>> GetRecentAsync(int count, CancellationToken cancellationToken = default)
    {
        return await _dbContext.EventLogs
            .AsNoTracking()
            .OrderByDescending(e => e.Timestamp)
            .Take(count)
            .ToListAsync(cancellationToken);
    }
}
