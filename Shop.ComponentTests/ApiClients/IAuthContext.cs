namespace Shop.ComponentTests.ApiClients;

public  interface IAuthContext
{
    public void AsCustomer();
    public void AsAccounting();
    public void AsAnonymous();
}