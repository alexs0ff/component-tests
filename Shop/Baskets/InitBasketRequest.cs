namespace Shop.Baskets;

public record class InitBasketRequest(OrderItem[] OrderItems, string PromoCode);