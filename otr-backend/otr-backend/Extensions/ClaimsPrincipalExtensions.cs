using System.Security.Claims;

namespace otr_backend.Extensions;

public static class ClaimsPrincipalExtensions
{
    public static uint GetUserId(this ClaimsPrincipal principal)
    {
        string sub = principal.FindFirstValue(ClaimTypes.NameIdentifier)!;
        return uint.Parse(sub);
    }
}
