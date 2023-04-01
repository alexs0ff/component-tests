using Shop.ComponentTests.Configurations;

namespace Shop.ComponentTests.ApiClients;

public class AuthContext : IAuthContext, ICurrentAuthToken
{
    private readonly TokensConfig _tokensConfig;

    public AuthContext(TokensConfig tokensConfig)
    {
        _tokensConfig = tokensConfig;
    }

    public void AsCustomer()
    {
        CurrentToken = _tokensConfig.Customer;
    }

    public void AsAccounting()
    {
        CurrentToken = _tokensConfig.Accounting;
    }

    public void AsAnonymous()
    {
        CurrentToken = string.Empty;
    }

    public string CurrentToken { get; private set; } = string.Empty;
}