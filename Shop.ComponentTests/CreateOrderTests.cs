using System.Net;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using MockServerClientNet.Model;
using MockServerClientNet.Verify;
using Shop.ComponentTests.ApiClients.Orders;
using Shop.ComponentTests.ApiClients.Shop;
using Shop.ComponentTests.Db.Models;
using Shop.ComponentTests.Tests;
using Shop.ComponentTests.Tests.Mocks;

namespace Shop.ComponentTests;

public sealed class CreateOrderTests : BaseFixture
{
    public CreateOrderTests(ServiceContext servicesContext) : base(servicesContext)
    {
    }

    [Fact]
    public async Task CreateOrder_Should_CreatePayment()
    {
        //Arrange
        await Prepare();
        var paymentResponse = new PaymentResponse
        {
            Status = PaymentStatus.New
        };
        await ServicesContext.MockServerClient.When(new HttpRequest()
                    .WithMethod(HttpMethod.Post)
                    .WithPath("/payment"),
                Times.Unlimited())
            .RespondAsync(new HttpResponse()
                .WithStatusCode(HttpStatusCode.OK)
                .WithBody(new JsonObjectContent<PaymentResponse>(paymentResponse)));

        var client = ServicesContext.ShopApiClient;
        var dbContext = await ServicesContext.DbContextFactory.CreateDbContextAsync();
        var user = new User
        {
            Id = Guid.NewGuid(),
            Name = "Customer"
        };
        dbContext.Users.Add(user);

        var item = new Db.Models.OrderItem
        {
            Id = Guid.NewGuid(),
            Count = 2,
            Name = "Name2",
            Price = 5.1M
        };
        var basket = new Basket
        {
            Id = Guid.NewGuid(), User = user, Userid = user.Id, PromoCode = "PROMO_CODE", Total = 10.2M,
            OrderItems =
            {
                item
            }
        };
        dbContext.Baskets.Add(basket);
        await dbContext.SaveChangesAsync();
        var payRequest = new PayOrderRequest
        {
            BasketId = basket.Id
        };

        //Act
        ServicesContext.AuthContext.AsCustomer();
        PayOrderResponse? response = await client.CreateOrderAsync(payRequest);

        //Assert
        dbContext = await ServicesContext.DbContextFactory.CreateDbContextAsync();
        basket = await dbContext.Baskets.Include(i => i.Order).SingleAsync();
        response.Should().BeEquivalentTo(new
        {
            basket.OrderId,
            PaymentUrl = $"https://mvshop.com/pavment?order={basket.OrderId}"
        });
        basket.Order.Should().BeEquivalentTo(new
        {
            Status = nameof(OrderStatus.Init)
        });
        await ServicesContext.MockServerClient.VerifyAsync(new HttpRequest()
            .WithMethod(HttpMethod.Post).WithPath("/payment")
            .WithBody(new JsonObjectMatcher<PaymentRequest>(new PaymentRequest
            {
                Amount = 10.2,
                Description = "Оплата",
                OrderId = response.OrderId
            })), VerificationTimes.Once());
    }
}