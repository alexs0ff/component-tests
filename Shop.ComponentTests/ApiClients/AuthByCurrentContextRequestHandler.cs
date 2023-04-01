using System.Net.Http.Headers;

namespace Shop.ComponentTests.ApiClients;

public class AuthByCurrentContextRequestHandler : DelegatingHandler
{
    private readonly ICurrentAuthToken _currentAuthToken;
    public AuthByCurrentContextRequestHandler(ICurrentAuthToken currentAuthToken)
    {
        _currentAuthToken = currentAuthToken;
    }
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        string token = _currentAuthToken.CurrentToken;
        if (!string.IsNullOrEmpty(token)) {
            AuthenticationHeaderValue authenticationHeaderValue = new AuthenticationHeaderValue("Bearer", token); request.Headers.Authorization = authenticationHeaderValue;
        }
        return await base.SendAsync(request, cancellationToken);
    }
}