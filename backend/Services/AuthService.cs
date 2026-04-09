using Microsoft.EntityFrameworkCore;
using RestaurantManagement.Api.Common.Enums;
using RestaurantManagement.Api.Data;
using RestaurantManagement.Api.DTOs.Auth;
using RestaurantManagement.Api.DTOs.Common;
using RestaurantManagement.Api.DTOs.Users;
using RestaurantManagement.Api.Entities;
using RestaurantManagement.Api.Helpers;
using RestaurantManagement.Api.Interfaces.Services;

namespace RestaurantManagement.Api.Services;

public class AuthService(
    ApplicationDbContext dbContext,
    IPasswordService passwordService,
    IJwtTokenService jwtTokenService) : IAuthService
{
    public async Task<AuthResponseDto> RegisterAsync(RegisterRequestDto request)
    {
        var normalizedEmail = request.Email.Trim().ToLowerInvariant();
        var exists = await dbContext.Users.AnyAsync(x => x.Email == normalizedEmail);
        if (exists)
        {
            throw new InvalidOperationException("A user with this email already exists.");
        }

        var (hash, salt) = passwordService.HashPassword(request.Password);
        var user = new AppUser
        {
            FullName = request.FullName.Trim(),
            Email = normalizedEmail,
            PasswordHash = hash,
            PasswordSalt = salt,
            Role = AppRoles.Customer,
            PhoneNumber = request.PhoneNumber,
            Address = request.Address,
            IsActive = true
        };

        await dbContext.Users.AddAsync(user);
        await dbContext.SaveChangesAsync();
        return CreateAuthResponse(user);
    }

    public async Task<AuthResponseDto> LoginAsync(LoginRequestDto request)
    {
        var normalizedEmail = request.Email.Trim().ToLowerInvariant();
        var user = await dbContext.Users.FirstOrDefaultAsync(x => x.Email == normalizedEmail);

        if (user is null || !passwordService.VerifyPassword(request.Password, user.PasswordHash, user.PasswordSalt))
        {
            throw new UnauthorizedAccessException("Invalid email or password.");
        }

        if (!user.IsActive)
        {
            throw new UnauthorizedAccessException("This account is inactive.");
        }

        user.LastLoginAtUtc = DateTime.UtcNow;
        await dbContext.SaveChangesAsync();
        return CreateAuthResponse(user);
    }

    public Task<ApiMessageDto> LogoutAsync()
    {
        return Task.FromResult(new ApiMessageDto("Logged out successfully. Remove the token from localStorage on the client."));
    }

    public async Task<UserDetailDto?> GetCurrentUserAsync(Guid userId)
    {
        var user = await dbContext.Users.AsNoTracking().FirstOrDefaultAsync(x => x.Id == userId);
        return user?.ToDetailDto();
    }

    private AuthResponseDto CreateAuthResponse(AppUser user)
    {
        var (token, expiresAtUtc) = jwtTokenService.GenerateToken(user);
        return new AuthResponseDto(token, expiresAtUtc, user.ToSummaryDto());
    }
}
