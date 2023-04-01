using System.Net;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using MockServerClientNet.Model;
using MockServerClientNet.Verify;
using Shop.ComponentTests.ApiClients.Shop;
using Shop.ComponentTests.Tests;
using Shop.ComponentTests.Tests.Mocks;

namespace Shop.ComponentTests
{
    public sealed class CreateBasketTests : BaseFixture
    {
        public CreateBasketTests(ServiceContext servicesContext) : base(servicesContext)
        {
        }

        [Fact]
        public async Task CreateBasket_Should_ReserveGoods()
        {
            //Arrange
            await Prepare();
            var reservationResponse = new ReservationResponse
            {
                Items = new List<ProductDto>
                {
                    new() { Count = 2, Name = "Name1", Price = 10 }
                }
            };
            await ServicesContext.MockServerClient.When(new HttpRequest()
                        .WithMethod(HttpMethod.Post).WithPath("/storage/reserve"),
                    Times.Unlimited())
                .RespondAsync(new HttpResponse()
                    .WithStatusCode(HttpStatusCode.OK)
                    .WithBody(new JsonObjectContent<ReservationResponse>(reservationResponse)));

            var client = ServicesContext.ShopApiClient;
            var dbContext = await ServicesContext.DbContextFactory.CreateDbContextAsync();
            var request = new InitBasketRequest
            {
                PromoCode = "PROMO_CODE",
                OrderItems = new List<OrderItem>
                {
                    new() { Count = 2, Name = "Name1" }
                }
            };

            //Act
            ServicesContext.AuthContext.AsCustomer();
            InitBasketResponse? response = await client.CreateBasketAsync(request);

            //Assert
            var basket = await dbContext.Baskets
                .Include(i => i.OrderItems)
                .Include(i => i.User)
                .SingleAsync();
            response.Should().BeEquivalentTo(new
            {
                BasketId = basket.Id,
                Total = 10M
            });
            basket.OrderItems.Should().BeEquivalentTo(new[]
            {
                new
                {
                    Name = "Name1",
                    Price = 10M,
                    Count = 2
                }
            });
            basket.User.Name.Should().Be("Customer");
            await ServicesContext.MockServerClient.VerifyAsync(
                new HttpRequest()
                    .WithMethod(HttpMethod.Post)
                    .WithPath("/storage/reserve"),
                VerificationTimes.Once());
        }
    }
}
