using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CafeCrm.Application.DTOs;
using CafeCrm.Domain.Entities;
using CafeCrm.Domain.Repositories;

namespace CafeCrm.Application.Services;

public class EventHistoryService : IEventHistoryService
{
    private readonly IEventLogRepository _eventLogs;

    public EventHistoryService(IEventLogRepository eventLogs)
    {
        _eventLogs = eventLogs;
    }

    public async Task LogAsync(string eventType, string message, string? details = null, CancellationToken cancellationToken = default)
    {
        var log = new EventLog(eventType, message, details);
        await _eventLogs.AddAsync(log, cancellationToken);
    }

    public async Task<IReadOnlyList<EventLogDto>> GetRecentAsync(int count = 50, CancellationToken cancellationToken = default)
    {
        var events = await _eventLogs.GetRecentAsync(count, cancellationToken);
        return events
            .Select(e => new EventLogDto(e.Timestamp, e.EventType, e.Message, e.Details))
            .ToList();
    }
}
