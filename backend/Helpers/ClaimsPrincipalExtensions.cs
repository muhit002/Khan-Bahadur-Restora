using System.Security.Claims;

namespace RestaurantManagement.Api.Helpers;

public static class ClaimsPrincipalExtensions
{
    public static Guid? GetUserId(this ClaimsPrincipal principal)
    {
        var rawValue = principal.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(rawValue, out var userId) ? userId : null;
    }

    public static string? GetRoleName(this ClaimsPrincipal principal) => principal.FindFirstValue(ClaimTypes.Role);

    public static string? GetEmail(this ClaimsPrincipal principal) => principal.FindFirstValue(ClaimTypes.Email);
}
