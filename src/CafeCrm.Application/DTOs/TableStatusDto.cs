using System;

namespace CafeCrm.Application.DTOs;

public class TableStatusDto
{
    public string TableNumber { get; init; } = string.Empty;
    public string CustomerName { get; init; } = string.Empty;
    public string? CustomerPhone { get; init; }
    public DateTime StartedAt { get; init; }
    public bool IsTakeaway { get; init; }
    public bool HasPosCheck { get; init; }
    public decimal CurrentAmount { get; init; }
    public int ItemCount { get; init; }
    public Guid VisitId { get; init; }

    public TimeSpan Duration => DateTime.UtcNow - StartedAt;
    public string DurationDisplay => $"{(int)Duration.TotalHours:00}:{Duration.Minutes:00}";
}
