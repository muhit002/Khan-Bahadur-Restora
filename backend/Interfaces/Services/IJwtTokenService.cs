using System.Security.Claims;
using RestaurantManagement.Api.Entities;

namespace RestaurantManagement.Api.Interfaces.Services;

public interface IJwtTokenService
{
    (string Token, DateTime ExpiresAtUtc) GenerateToken(AppUser user);
    ClaimsPrincipal? ValidateToken(string token);
}
