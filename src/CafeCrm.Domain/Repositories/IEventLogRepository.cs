using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CafeCrm.Domain.Entities;

namespace CafeCrm.Domain.Repositories;

public interface IEventLogRepository
{
    Task AddAsync(EventLog log, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<EventLog>> GetRecentAsync(int count, CancellationToken cancellationToken = default);
}
