using Microsoft.EntityFrameworkCore;
using Shop.Entities;

namespace Shop.Infrastructure;

public static class CreateDatabaseOnStartup
{
    public static void CreateDb(this IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider
            .GetRequiredService<ShopContext>();
        dbContext.Database.Migrate();
    }
}