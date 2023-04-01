using System.Diagnostics.Contracts;

namespace Shop.Entities;

public class OrderItem
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Count { get; set; }
    public decimal Price { get; set; }
    public Guid BasketId { get; set; }
    public Basket Basket { get; set; } = default!;
}