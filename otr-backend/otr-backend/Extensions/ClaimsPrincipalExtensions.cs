using System.Security.Claims;

namespace otr_backend.Extensions;

/// <summary>Helpers for reading the authenticated user from the JWT claims.</summary>
public static class ClaimsPrincipalExtensions
{
    /// <summary>
    /// Returns the database id of the authenticated user, stored in the
    /// <see cref="ClaimTypes.NameIdentifier"/> claim by <c>JwtTokenService</c>.
    /// Only call this from endpoints protected by <c>[Authorize]</c>.
    /// </summary>
    public static uint GetUserId(this ClaimsPrincipal principal)
    {
        string subject = principal.FindFirstValue(ClaimTypes.NameIdentifier)!;
        return uint.Parse(subject);
    }
}
