using System;

namespace CafeCrm.Application.DTOs;

public class VisitDetailDto
{
    public Guid Id { get; init; }
    public string CustomerName { get; init; } = string.Empty;
    public string? CustomerPhone { get; init; }
    public string TableNumber { get; init; } = string.Empty;
    public DateTime StartedAt { get; init; }
    public string? Notes { get; init; }
    public bool IsTakeaway { get; init; }
    public string? PosCheckId { get; init; }
    public OrderItemDto[] OrderItems { get; init; } = Array.Empty<OrderItemDto>();
    public decimal TotalAmount { get; init; }
    public TimeSpan Duration { get; init; }
    public string DurationDisplay => $"{(int)Duration.TotalHours:00}:{Duration.Minutes:00}";
    public int ItemCount => OrderItems?.Length ?? 0;
}
