using FluentAssertions;
using Shop.ComponentTests.ApiClients.Shop;
using Shop.ComponentTests.Db.Models;
using Shop.ComponentTests.Tests;
using OrderItem = Shop.ComponentTests.ApiClients.Shop.OrderItem;

namespace Shop.ComponentTests;

public class GetOrderTests : BaseFixture
{
    public GetOrderTests(ServiceContext servicesContext) : base(servicesContext)
    {
    }

    [Fact]
    public async Task GetOrder_Should_ReturnsDtoFromDb()
    {
        //Arrange
        await Prepare();
        var client = ServicesContext.ShopApiClient;
        var dbContext = await ServicesContext.DbContextFactory.CreateDbContextAsync();
        var user = new User { Id = Guid.NewGuid(), Name = "Customer" };
        dbContext.Users.Add(user);

        var item = new Db.Models.OrderItem { Id = Guid.NewGuid(), Count = 2, Name = "Name1", Price = 5.1M };
        var basket = new Basket
        {
            Id = Guid.NewGuid(),
            User = user,
            Userid = user.Id,
            PromoCode = "PROMO_CODE",
            Total = 10.2M,
            OrderItems = { item }
        };
        dbContext.Baskets.Add(basket);
        await dbContext.SaveChangesAsync();

        //Act
        ServicesContext.AuthContext.AsCustomer();
        BasketDto? response = await client.GetBasketAsync(basket.Id.ToString());

        //Assert
        response.Should().BeEquivalentTo(new
        {
            basket.Id,
            basket.PromoCode,
            basket.Total,
            OrderStatus = (OrderStatus?)null,
            Items = new[]
            {
                new OrderItem
                {
                    Count = item.Count, Name = item.Name
                }
            }
        });
    }

    [Fact]
    public async Task GetOrder_Should_ChecksUserBasket()
    {
        //Arrange
        await Prepare();
        var client = ServicesContext.ShopApiClient;
        var dbContext = await ServicesContext.DbContextFactory.CreateDbContextAsync();
        var user = new User { Id = Guid.NewGuid(), Name = "Customer1" };
        dbContext.Users.Add(user);

        var item = new Db.Models.OrderItem { Id = Guid.NewGuid(), Count = 2, Name = "Name1", Price = 5.1M };
        var basket = new Basket
        {
            Id = Guid.NewGuid(),
            User = user,
            Userid = user.Id,
            PromoCode = "PROMO_CODE",
            Total = 10.2M,
            OrderItems = { item }
        };
        dbContext.Baskets.Add(basket);
        await dbContext.SaveChangesAsync();

        //Act
        ServicesContext.AuthContext.AsCustomer();
        var task = () => client.GetBasketAsync(basket.Id.ToString());

        //Assert
        var apiException = await task.Should().ThrowAsync<ApiException>();
        apiException.And.StatusCode.Should().Be(404);

    }
}