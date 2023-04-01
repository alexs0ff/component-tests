using System.Text.Json;
using System.Text.Json.Serialization;
using Goods.Storage;
using Microsoft.AspNetCore.Http.Json;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddScoped<StorageService>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c => { c.UseAllOfToExtendReferenceSchemas(); });
builder.Services.Configure<JsonOptions>(options =>
    {
        options.SerializerOptions.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));
    }
);
var app = builder.Build();
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapPost("/storage/reserve",
        async (ReservationRequest request, StorageService service) => await service.Reserve(request))
    .Produces<ReservationResponse>().WithName("StorageReserve").WithOpenApi();
app.MapPost("/storage/release",
        async (ReleaseRequest request, StorageService service) => await service.Release(request))
    .Produces<ReleaseResponse>()
    .WithName("StorageRelease")
    .WithOpenApi();
app.Run();