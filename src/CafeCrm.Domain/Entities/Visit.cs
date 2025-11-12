using System;
using System.Collections.Generic;
using System.Linq;
using CafeCrm.Domain.Enums;

namespace CafeCrm.Domain.Entities;

public class Visit : AuditableEntity
{
    private readonly List<OrderItem> _orderItems = new();

    public Guid? CustomerId { get; internal set; }
    public Customer? Customer { get; internal set; }
    public string TableNumber { get; private set; } = string.Empty;
    public DateTime StartedAt { get; private set; }
    public DateTime? EndedAt { get; private set; }
    public string? Notes { get; private set; }
    public bool IsTakeaway { get; private set; }
    public decimal TotalAmount { get; private set; }
    public VisitSource Source { get; private set; } = VisitSource.Unknown;
    public string? PosCheckId { get; private set; }
    public DateTime? PosCheckClosedAt { get; private set; }

    public IReadOnlyCollection<OrderItem> OrderItems => _orderItems.AsReadOnly();

    private Visit()
    {
    }

    public Visit(Guid? customerId, string tableNumber, string? notes, bool isTakeaway, VisitSource source = VisitSource.Unknown)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(tableNumber);

        CustomerId = customerId;
        TableNumber = tableNumber.Trim();
        Notes = string.IsNullOrWhiteSpace(notes) ? null : notes.Trim();
        IsTakeaway = isTakeaway;
        StartedAt = DateTime.UtcNow;
        Source = source;
    }

    public void AssignPosCheck(string posCheckId)
    {
        PosCheckId = posCheckId;
    }

    public void ClosePosCheck(decimal totalAmount, DateTime closedAt)
    {
        PosCheckClosedAt = closedAt;
        TotalAmount = totalAmount;
        EndedAt = closedAt;
    }

    public void AddOrderItem(string name, int quantity, decimal price)
    {
        if (quantity <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(quantity));
        }

        if (price < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(price));
        }

        var item = new OrderItem(name, quantity, price);
        _orderItems.Add(item);
        TotalAmount += item.Total;
    }

    public void Complete(decimal? finalAmount = null, DateTime? completedAt = null)
    {
        if (EndedAt.HasValue)
        {
            return;
        }

        if (finalAmount.HasValue)
        {
            if (finalAmount.Value < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(finalAmount));
            }

            TotalAmount = finalAmount.Value;
        }
        else if (TotalAmount <= 0 && _orderItems.Count > 0)
        {
            TotalAmount = _orderItems.Sum(o => o.Total);
        }

        EndedAt = completedAt ?? DateTime.UtcNow;
    }
}
