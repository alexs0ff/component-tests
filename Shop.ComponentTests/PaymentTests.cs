using System.Net;
using FluentAssertions;
using MockServerClientNet.Model;
using MockServerClientNet.Verify;
using Shop.ComponentTests.ApiClients.Orders;
using Shop.ComponentTests.ApiClients.Shop;
using Shop.ComponentTests.Db.Models;
using Shop.ComponentTests.Tests;
using Shop.ComponentTests.Tests.Mocks;
using OrderItem = Shop.ComponentTests.Db.Models.OrderItem;

namespace Shop.ComponentTests;

public sealed class PaymentTests : BaseFixture
{
    public PaymentTests(ServiceContext servicesContext) : base(servicesContext)
    {
    }

    [Fact]
    public async Task CheckPayment_ShouldNot_AvailableForCustomer()
    {
        //Arrange
        await Prepare();
        var client = ServicesContext.ShopApiClient;

        //Act
        ServicesContext.AuthContext.AsCustomer();
        var task = () => client.InternalCheckOrderStatusAsync(Guid.NewGuid().ToString());

        //Assert
        var apiException = await task.Should().ThrowAsync<ApiException>();
        apiException.And.StatusCode.Should().Be(403);
    }

    [Fact]
    public async Task CheckPayment_Should_ReleaseItems_WhenPaymentFail()
    {
        //Arrange
        await Prepare();

        var paymentResponse = new PaymentDto
        {
            Status = PaymentStatus.Fail,
            OrderId = Guid.NewGuid()
        };
        await ServicesContext.MockServerClient.When(new HttpRequest()
                .WithMethod(HttpMethod.Get)
                .WithPath("/payment/.*")
            , Times.Unlimited()
        ).RespondAsync(new HttpResponse()
            .WithStatusCode(HttpStatusCode.OK)
            .WithBody(new JsonObjectContent<PaymentDto>(paymentResponse)));

        await ServicesContext.MockServerClient.When(new HttpRequest()
                .WithMethod(HttpMethod.Post)
                .WithPath("/storage/release")
            , Times.Unlimited()
        ).RespondAsync(new HttpResponse()
            .WithStatusCode(HttpStatusCode.OK)
            .WithBody(new JsonObjectContent<ReleaseResponse>(new ReleaseResponse
            {
                Total = 2
            })));

        var client = ServicesContext.ShopApiClient;
        var dbContext = await ServicesContext.DbContextFactory.CreateDbContextAsync();

        var user = new User
        {
            Id = Guid.NewGuid(),
            Name = "Customer"
        };
        dbContext.Users.Add(user);
        var item = new OrderItem
        {
            Id = Guid.NewGuid(),
            Count = 2,
            Name = "Name2",
            Price = 5.1M
        };

        var basket = new Basket
        {
            Id = Guid.NewGuid(),
            User = user,
            Userid = user.Id,
            PromoCode = "PROMO_CODE",
            Total = .2M,
            OrderItems = { item },
            Order = new Order
            {
                Id = paymentResponse.OrderId,
                Status = nameof(OrderStatus.Init)
            },
            OrderId = paymentResponse.OrderId
        };
        dbContext.Baskets.Add(basket);
        dbContext.Orders.Add(basket.Order);
        await dbContext.SaveChangesAsync();

        //Act
        ServicesContext.AuthContext.AsAccounting();
        await client.InternalCheckOrderStatusAsync(basket.Id.ToString());

        //Assert

        await ServicesContext.MockServerClient.VerifyAsync(new HttpRequest()
                .WithMethod(HttpMethod.Get)
                .WithPath($"/payment/{basket.Order.Id}"),
            VerificationTimes.Once());

        await ServicesContext.MockServerClient
            .VerifyAsync(new HttpRequest().WithMethod(HttpMethod.Post)
                .WithPath($"/storage/release")
                .WithBody(new JsonObjectMatcher<ReleaseRequest>(new ReleaseRequest
                {
                    Items = new List<ReservationItem>
                    {
                        new()
                        {
                            Count = 2, Name = "Name2"
                        }
                    }
                })), VerificationTimes.Once());
    }
}