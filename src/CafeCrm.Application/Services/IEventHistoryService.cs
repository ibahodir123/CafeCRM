using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CafeCrm.Application.DTOs;

namespace CafeCrm.Application.Services;

public interface IEventHistoryService
{
    Task LogAsync(string eventType, string message, string? details = null, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<EventLogDto>> GetRecentAsync(int count = 50, CancellationToken cancellationToken = default);
}
