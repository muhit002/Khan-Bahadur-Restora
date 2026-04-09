using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace RestaurantManagement.Api.Helpers;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public sealed class AppAuthorizeAttribute : Attribute, IAsyncAuthorizationFilter
{
    public string Roles { get; set; } = string.Empty;

    public Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        var allowAnonymous = context.ActionDescriptor.EndpointMetadata.OfType<AllowAnonymousAttribute>().Any();
        if (allowAnonymous)
        {
            return Task.CompletedTask;
        }

        var user = context.HttpContext.User;
        if (user.Identity?.IsAuthenticated != true)
        {
            context.Result = new JsonResult(new { message = "Authentication is required." })
            {
                StatusCode = StatusCodes.Status401Unauthorized
            };
            return Task.CompletedTask;
        }

        if (string.IsNullOrWhiteSpace(Roles))
        {
            return Task.CompletedTask;
        }

        var allowedRoles = Roles.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        var userRole = user.FindFirstValue(ClaimTypes.Role);
        if (string.IsNullOrWhiteSpace(userRole) || !allowedRoles.Contains(userRole, StringComparer.OrdinalIgnoreCase))
        {
            context.Result = new JsonResult(new { message = "You do not have permission to access this resource." })
            {
                StatusCode = StatusCodes.Status403Forbidden
            };
        }

        return Task.CompletedTask;
    }
}
