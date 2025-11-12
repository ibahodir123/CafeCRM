using System;
using System.Collections.Generic;

namespace CafeCrm.Domain.Entities;

public class PosTicket
{
    public string TicketId { get; init; } = Guid.NewGuid().ToString("N");
    public decimal Total { get; init; }
    public DateTime OpenedAt { get; init; } = DateTime.UtcNow;
    public DateTime? ClosedAt { get; init; }
    public string? Cashier { get; init; }
    public string? PaymentMethod { get; init; }
    public string? TableNumber { get; init; }
    public IDictionary<string, decimal> Items { get; init; } = new Dictionary<string, decimal>();
}
