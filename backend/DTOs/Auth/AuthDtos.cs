using System.ComponentModel.DataAnnotations;
using RestaurantManagement.Api.DTOs.Users;

namespace RestaurantManagement.Api.DTOs.Auth;

public class LoginRequestDto
{
    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string Password { get; set; } = string.Empty;
}

public class RegisterRequestDto
{
    [Required, StringLength(120)]
    public string FullName { get; set; } = string.Empty;

    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required, MinLength(8)]
    public string Password { get; set; } = string.Empty;

    [Phone]
    public string? PhoneNumber { get; set; }

    public string? Address { get; set; }
}

public record AuthResponseDto(string Token, DateTime ExpiresAtUtc, UserSummaryDto User);
