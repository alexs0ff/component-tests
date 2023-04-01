using System.Security.Claims;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Shop.ApiClients;
using Shop.Baskets;
using Shop.Entities;
using Shop.Extensions;
using Shop.Infrastructure;
using Shop.Orders;
using Shop.Users;
using JsonOptions = Microsoft.AspNetCore.Http.Json.JsonOptions;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddAuthorization(cfg =>
    cfg.AddPolicy("accounting_policy", policy => { policy.RequireRole("accounting"); }));
builder.Services.AddAuthentication("Bearer").AddJwtBearer();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(s =>
{
    s.SchemaFilter<EnumSchemaFilter>();
    s.UseAllOfToExtendReferenceSchemas();
    s.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = builder.Environment.ApplicationName, Version = "v1"
    });
    s.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme (Example: ’Bearer 12345abcdef’",
        Name = "Authorization", In = ParameterLocation.Header, Type = SecuritySchemeType.ApiKey, Scheme = "Bearer"
    });
    s.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme, Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});
builder.Services.Configure<JsonOptions>(options =>
{
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));
});
builder.Services.AddDbContext<ShopContext>(opt => opt.UseNpgsql(builder.Configuration["ConnectionStrings:Shop"]));
builder.Services.AddScoped<BasketService>();
builder.Services.AddScoped<OrderService>();
builder.Services.AddScoped<UserService>();
builder.Services.AddHttpClient<GoodsApiClient>(cfg =>
    cfg.BaseAddress = new Uri(builder.Configuration["ServiceUrls:Goods"] ??
                              throw new InvalidOperationException("ServiceUrIs:Goods is null")));
builder.Services.AddHttpClient<OrdersApiClient>(cfg =>
    cfg.BaseAddress = new Uri(builder.Configuration["ServiceUrls:Orders"] ??
                              throw new InvalidOperationException("ServiceUrls:Orders is null")));

var app = builder.Build();
app.UseAuthorization();
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseExceptionHandler(exceptionHandlerApp
    => exceptionHandlerApp.Run(
        async context => { await Results.Problem().ExecuteAsync(context); }));
app.MapPost("/basket",
        async ([FromBody] InitBasketRequest request, ClaimsPrincipal user, BasketService basketService) =>
            await basketService.Create(request, user.GetName())).Produces<InitBasketResponse>().RequireAuthorization()
    .WithName("CreateBasket");
app.MapGet("/basket/{basketId}", async (Guid basketId, ClaimsPrincipal user, BasketService basketService) =>
        await basketService.ResultOnExceptionAsync(async () => await basketService.Get(basketId, user.GetName())))
    .RequireAuthorization()
    .Produces<BasketDto>()
    .WithName("GetBasket");
app.MapPost("/order", async (PayOrderRequest request, ClaimsPrincipal user, OrderService orderService) =>
        await orderService.ResultOnExceptionAsync(async () => await orderService.Pay(request, user.GetName())))
    .RequireAuthorization().Produces<PayOrderResponse>()
    .WithName("CreateOrder");
app.MapPost("internal/order/status/{basketId}", async (Guid basketId, OrderService orderService) =>
        await orderService.ResultOnExceptionAsync(async () => await orderService.CheckPaymentStatus(basketId)))
    .RequireAuthorization("accounting_policy").Produces<OrderStatusResponse>().Produces(StatusCodes.Status200OK)
    .WithName("InternalCheckOrderStatus");
app.Services.CreateDb();
app.Run();