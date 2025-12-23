using System.Security.Claims;

namespace CobranzaCloud.Api.Extensions;

/// <summary>
/// Extension methods for ClaimsPrincipal to support multi-tenant operations
/// </summary>
public static class ClaimsPrincipalExtensions
{
    public static Guid GetOrganizationId(this ClaimsPrincipal user)
    {
        var claim = user.FindFirst("org_id")
            ?? throw new UnauthorizedAccessException("Organization claim not found");
        return Guid.Parse(claim.Value);
    }

    public static Guid GetUserId(this ClaimsPrincipal user)
    {
        var claim = user.FindFirst(ClaimTypes.NameIdentifier)
            ?? throw new UnauthorizedAccessException("User claim not found");
        return Guid.Parse(claim.Value);
    }

    public static string GetUserEmail(this ClaimsPrincipal user)
    {
        var claim = user.FindFirst(ClaimTypes.Email)
            ?? throw new UnauthorizedAccessException("Email claim not found");
        return claim.Value;
    }

    public static string? GetUserRole(this ClaimsPrincipal user)
    {
        return user.FindFirst(ClaimTypes.Role)?.Value;
    }
}
