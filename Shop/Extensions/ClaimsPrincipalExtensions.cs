using System.Security.Authentication;
using System.Security.Claims;

namespace Shop.Extensions;

internal static class ClaimsPrincipalExtensions
{
    public static string GetName(this ClaimsPrincipal principal)
    {
        if (principal.Identity?.Name is null) throw new AuthenticationException();
        return principal.Identity.Name;
    }
}