using Microsoft.EntityFrameworkCore;
using Shop.ApiClients;
using Shop.Entities;
using Shop.Exceptions;
using Shop.Mappings;
using Shop.Users;

namespace Shop.Baskets;

public class BasketService
{
    private readonly GoodsApiClient _goodsApiClient;
    private readonly ShopContext _shopContext;
    private readonly UserService _userService;

    public BasketService(ShopContext shopContext, UserService userService, GoodsApiClient goodsApiClient)
    {
        _shopContext = shopContext;
        _userService = userService;
        _goodsApiClient = goodsApiClient;
    }

    public async Task<InitBasketResponse> Create(InitBasketRequest request, string userName)
    {
        var user = await _userService.CreateIfNeeded(userName);
        var id = Guid.NewGuid();
        var reservationResponse = await _goodsApiClient.StorageReserveAsync(new ReservationRequest
        {
            Items = request.OrderItems.Select(i => i.ToReservationItem()).ToArray()
        });
        var items = new List<Entities.OrderItem>();
        foreach (var requestorderItem in request.OrderItems)
        {
            var responseItem = reservationResponse.Items.Single(i => i.Name == requestorderItem.Name);
            items.Add(new Entities.OrderItem
            {
                Name = requestorderItem.Name,
                BasketId = id,
                Id = Guid.NewGuid(),
                Count = responseItem.Count,
                Price = (decimal)responseItem.Price
            });
        }

        var basket = new Basket
        {
            Id = id,
            PromoCode = request.PromoCode,
            Userid = user.Id,
            OrderItems = items
        };
        basket.Total = basket.OrderItems.Sum(i => i.Price);
        _shopContext.Baskets.Add(basket);
        await _shopContext.SaveChangesAsync();
        return new InitBasketResponse(basket.Id, basket.Total);
    }

    public async Task<BasketDto> Get(Guid basketId, string userName)
    {
        var basket = await _shopContext.Baskets
            .Include(i => i.OrderItems)
            .Include(i => i.Order)
            .AsNoTracking()
            .FirstOrDefaultAsync(i => i.User.Name == userName && i.Id == basketId);
        if (basket == null) throw new EntityNotFoundException($"Basket not found: {basketId}", "Basket");

        return basket.ToDto();
    }
}