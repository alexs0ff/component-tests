using Shop.Orders;

namespace Shop.Entities;

public class Order
{
    public Guid Id { get; set; }
    public Guid BasketId { get; set; }
    public Basket Basket { get; set; } = default!;
    public OrderStatus Status { get; set; }

}