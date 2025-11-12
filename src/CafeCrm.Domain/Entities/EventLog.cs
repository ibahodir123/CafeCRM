using System;

namespace CafeCrm.Domain.Entities;

public class EventLog : AuditableEntity
{
    public string EventType { get; private set; } = string.Empty;
    public string Message { get; private set; } = string.Empty;
    public string? Details { get; private set; }
    public DateTime Timestamp { get; private set; } = DateTime.UtcNow;

    private EventLog()
    {
    }

    public EventLog(string eventType, string message, string? details = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(eventType);
        ArgumentException.ThrowIfNullOrWhiteSpace(message);

        EventType = eventType.Trim();
        Message = message.Trim();
        Details = string.IsNullOrWhiteSpace(details) ? null : details.Trim();
    }
}
