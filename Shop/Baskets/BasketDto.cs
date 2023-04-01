using Shop.Orders;

namespace Shop.Baskets;

public record class BasketDto(Guid Id, OrderItem[] Items, string PromoCode, decimal Total, OrderStatus? OrderStatus);