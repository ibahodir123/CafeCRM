namespace CafeCrm.Application.DTOs;

public class OrderItemDto
{
    public string Name { get; init; } = string.Empty;
    public int Quantity { get; init; }
    public decimal Price { get; init; }
    public decimal Total { get; init; }
}
