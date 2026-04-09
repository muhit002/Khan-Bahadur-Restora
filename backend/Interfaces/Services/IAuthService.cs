using RestaurantManagement.Api.DTOs.Auth;
using RestaurantManagement.Api.DTOs.Common;
using RestaurantManagement.Api.DTOs.Users;

namespace RestaurantManagement.Api.Interfaces.Services;

public interface IAuthService
{
    Task<AuthResponseDto> RegisterAsync(RegisterRequestDto request);
    Task<AuthResponseDto> LoginAsync(LoginRequestDto request);
    Task<ApiMessageDto> LogoutAsync();
    Task<UserDetailDto?> GetCurrentUserAsync(Guid userId);
}
