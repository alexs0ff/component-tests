using Microsoft.EntityFrameworkCore;
using Shop.ApiClients;
using Shop.Entities;
using Shop.Exceptions;

namespace Shop.Orders;

public class OrderService
{
    private readonly ShopContext _shopContext;
    private readonly OrdersApiClient _ordersApiClient;
    private readonly GoodsApiClient _goodsApiClient;

    public OrderService(ShopContext shopContext, OrdersApiClient ordersApiClient, GoodsApiClient goodsApiClient)
    {
        _shopContext = shopContext;
        _ordersApiClient = ordersApiClient;
        _goodsApiClient = goodsApiClient;
    }

    public async Task<PayOrderResponse> Pay(PayOrderRequest request, string userName)
    {
        var basket = await _shopContext.Baskets.Include(i => i.Order)
            .FirstOrDefaultAsync(i => i.Id == request.BasketId && i.User.Name == userName);
        if (basket == null)
        {
            throw new EntityNotFoundException("Basket not found", nameof(Basket));
        }

        if (basket.OrderId is not null)
        {
            return new PayOrderResponse(basket.OrderId.Value,
                $"https://mvshop.com/pavment?order={basket.OrderId.Value}");
        }

        basket.Order = new Order { Id = Guid.NewGuid(), BasketId = basket.Id, Status = OrderStatus.Init };
        _shopContext.Orders.Add(basket.Order);
        basket.OrderId = basket.Order.Id;
        var paymentResponse = await _ordersApiClient.CreatePaymentAsync(new PaymentRequest
        {
            Amount = (double)basket.Total,
            Description = "Оплата",
            OrderId = basket.Order.Id
        });
        await _shopContext.SaveChangesAsync();
        return new PayOrderResponse(basket.OrderId.Value,
            $"https://mvshop.com/pavment?order={basket.OrderId.Value}");
    }

    public async Task<OrderStatusResponse> CheckPaymentStatus(Guid basketId)
    {
        var basket = await _shopContext.Baskets.Include(i => i.Order).Include(i => i.OrderItems)
            .FirstOrDefaultAsync(i => i.Id == basketId);
        if (basket == null)
        {
            throw new EntityNotFoundException("Basket not found", nameof(Basket));
        }

        if (basket.OrderId is null)
        {
            throw new OrderNotInitializedException("Order not initialized", basketId);
        }

        var statusResponse = await _ordersApiClient.GetPaymentAsync(basket.OrderId.Value.ToString());
        OrderStatus? status;
        bool needRelease = false;
        switch (statusResponse.Status)
        {
            case PaymentStatus.New:
                status = OrderStatus.Init;
                break;
            case PaymentStatus.Paid:
                status = OrderStatus.Paid;
                break;
            case PaymentStatus.Fail:
                status = OrderStatus.Fail;
                needRelease = true;
                break;
            case PaymentStatus.Refunded:
                status = OrderStatus.Refunded;
                needRelease = true;
                break;
            case PaymentStatus.Canceled:
                status = OrderStatus.Fail;
                needRelease = true;
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        if (basket.Order is not null)
        {
            if (needRelease)
            {
                await _goodsApiClient.StorageReleaseAsync(new ReleaseRequest
                {
                    Items = basket.OrderItems.Select(i => new ReservationItem
                    {
                        Count = i.Count,
                        Name = i.Name
                    }).ToArray()
                });
            }

            basket.Order.Status = status.Value;
            await _shopContext.SaveChangesAsync();
        }

        return new OrderStatusResponse(status);
    }
}