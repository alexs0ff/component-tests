using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MockServerClientNet;
using Shop.ComponentTests.ApiClients;
using Shop.ComponentTests.ApiClients.Shop;
using Shop.ComponentTests.Configurations;
using Shop.ComponentTests.Db;

namespace Shop.ComponentTests.Tests;

public sealed class ServiceContext : IDisposable
{
    private readonly ServiceProvider _serviceProvider;
    public DatabaseConfig DatabaseConfig { get; }
    public TestsConfig TestsConfig { get; }

    public IDbContextFactory<ShopContext> DbContextFactory =>
        _serviceProvider.GetRequiredService<IDbContextFactory<ShopContext>>();

    public IAuthContext AuthContext => _serviceProvider.GetRequiredService<IAuthContext>();
    public ShopApiClient ShopApiClient => _serviceProvider.GetRequiredService<ShopApiClient>();
    public MockServerClient MockServerClient { get; }

    public ServiceContext()
    {
        var testsConfiguration =
            new ConfigurationBuilder().AddJsonFile("testsettings.json").AddJsonFile("tokens.json")
                .AddEnvironmentVariables()
                .Build();
        DatabaseConfig = testsConfiguration.GetSection("DatabaseConfig").Get<DatabaseConfig>() ??
                         throw new Exception("DatabaseConfig is null");
        TestsConfig = testsConfiguration.GetSection("TestsConfig").Get<TestsConfig>() ??
                      throw new Exception("TestsConfig is null");
        var tokensConfig = testsConfiguration.GetSection("TestJwtTokens").Get<TokensConfig>() ??
                           throw new Exception("TokensConfig is null");

        var servicecollection = new ServiceCollection();
        servicecollection.AddSingleton(tokensConfig);
        servicecollection.AddScoped<AuthByCurrentContextRequestHandler>();
        var authContext = new AuthContext(tokensConfig);
        servicecollection.AddSingleton<IAuthContext>(authContext);
        servicecollection.AddSingleton<ICurrentAuthToken>(authContext);
        servicecollection.AddDbContextFactory<ShopContext>(opt => { opt.UseNpgsql(DatabaseConfig.ConnectionString); });
        servicecollection.AddHttpClient<ShopApiClient>
                (cfg => cfg.BaseAddress = TestsConfig.ShopUrl)
            .AddHttpMessageHandler<AuthByCurrentContextRequestHandler>();
        MockServerClient = new MockServerClient(TestsConfig.MockServerUrl.Host, TestsConfig.MockServerUrl.Port);
        _serviceProvider = servicecollection.BuildServiceProvider();
    }

    public void Dispose()
    {
        _serviceProvider.Dispose();
        MockServerClient.Dispose();
    }
}