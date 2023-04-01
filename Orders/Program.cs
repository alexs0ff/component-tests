using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Http.Json;
using Orders.Infrastructure;
using Orders.Payments;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddScoped<PaymentService>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SchemaFilter<EnumSchemaFilter>();
    c.UseAllOfToExtendReferenceSchemas();
});
builder.Services.Configure<JsonOptions>(options =>
{
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));
});
var app = builder.Build();
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapPost("/payment", async (PaymentRequest request, PaymentService service) => await service.CreatePayment(request))
    .Produces<PaymentResponse>().WithName("CreatePayment").WithOpenApi();
app.MapGet("/payment/{orderId}", async (Guid orderId, PaymentService service) =>
{
    var result = await service.Get(orderId);
    return result is null ? Results.NotFound() : Results.Ok(result);
}).Produces<PaymentDto>().Produces(StatusCodes.Status404NotFound).WithName("GetPayment").WithOpenApi();

app.MapPut("/internal/payment/{orderId}/{status}",
        async (Guid orderId, PaymentStatus newStatus, PaymentService service) =>
        {
            var result = await service.SetStatus(orderId, newStatus);
            return result is null ? Results.Conflict() : Results.Ok(result);
        }).WithName("SetPaymentStatus").Produces<PaymentDto>()
    .Produces(StatusCodes.Status409Conflict).WithOpenApi();
app.Run();