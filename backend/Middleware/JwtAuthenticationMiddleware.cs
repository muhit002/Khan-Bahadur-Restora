using RestaurantManagement.Api.Interfaces.Services;

namespace RestaurantManagement.Api.Middleware;

public class JwtAuthenticationMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context, IJwtTokenService jwtTokenService)
    {
        var authHeader = context.Request.Headers.Authorization.ToString();
        if (!string.IsNullOrWhiteSpace(authHeader) && authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
        {
            var token = authHeader["Bearer ".Length..].Trim();
            var principal = jwtTokenService.ValidateToken(token);
            if (principal is not null)
            {
                context.User = principal;
            }
        }

        await next(context);
    }
}
