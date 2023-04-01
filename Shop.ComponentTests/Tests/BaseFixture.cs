using Microsoft.EntityFrameworkCore;
using Shop.ComponentTests.Db;

namespace Shop.ComponentTests.Tests;

public abstract class BaseFixture : IClassFixture<ServiceContext>
{
    protected readonly ServiceContext ServicesContext;

    protected BaseFixture(ServiceContext servicesContext)
    {
        ServicesContext = servicesContext;
    }

    protected async Task Prepare(CancellationToken cancellationToken = default)
    {
        ShopContext context = await ServicesContext.DbContextFactory.CreateDbContextAsync(cancellationToken);
        foreach (string tableName in ServicesContext.DatabaseConfig.TablesToClean.Split(",",
                     StringSplitOptions.RemoveEmptyEntries))
        {
            await context.Database.ExecuteSqlRawAsync($"DELETE FROM public.\"{tableName.Trim()}\"", cancellationToken);
        }

        ServicesContext.AuthContext.AsAnonymous();
        await ServicesContext.MockServerClient.ResetAsync();
    }
}