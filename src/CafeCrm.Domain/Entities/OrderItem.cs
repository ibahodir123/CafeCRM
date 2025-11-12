using System;

namespace CafeCrm.Domain.Entities;

public class OrderItem : AuditableEntity
{
    public string Name { get; private set; } = string.Empty;
    public int Quantity { get; private set; }
    public decimal Price { get; private set; }
    public decimal Total => Quantity * Price;

    private OrderItem()
    {
    }

    public OrderItem(string name, int quantity, decimal price)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        if (quantity <= 0) throw new ArgumentOutOfRangeException(nameof(quantity));
        if (price < 0) throw new ArgumentOutOfRangeException(nameof(price));

        Name = name.Trim();
        Quantity = quantity;
        Price = price;
    }
}
