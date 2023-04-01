using Shop.ApiClients;
using Shop.Baskets;
using Shop.Entities;
using OrderItem = Shop.Baskets.OrderItem;

namespace Shop.Mappings;

public static class BasketMap
{
    public static BasketDto ToDto(this Basket basket)
    {
        return new BasketDto(
            basket.Id, basket.OrderItems.Select(i => new OrderItem(i.Name, i.Count)).ToArray(), basket.PromoCode, basket.Total, basket.Order?.Status);
    }

    public static ReservationItem ToReservationItem(this OrderItem item)
    {
        return new ReservationItem { Count = item.Count, Name = item.Name };
    }
}