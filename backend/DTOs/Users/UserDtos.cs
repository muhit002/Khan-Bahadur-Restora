using System.ComponentModel.DataAnnotations;

namespace RestaurantManagement.Api.DTOs.Users;

public record UserSummaryDto(
    Guid Id,
    string FullName,
    string Email,
    string Role,
    bool IsActive,
    string? PhoneNumber);

public record UserDetailDto(
    Guid Id,
    string FullName,
    string Email,
    string Role,
    bool IsActive,
    string? PhoneNumber,
    string? Address,
    DateTime CreatedAtUtc,
    DateTime? LastLoginAtUtc);

public class CreateUserDto
{
    [Required, StringLength(120)]
    public string FullName { get; set; } = string.Empty;

    [Required, EmailAddress, StringLength(160)]
    public string Email { get; set; } = string.Empty;

    [Required, MinLength(8)]
    public string Password { get; set; } = string.Empty;

    [Required]
    public string Role { get; set; } = string.Empty;

    [Phone]
    public string? PhoneNumber { get; set; }

    [StringLength(250)]
    public string? Address { get; set; }
}

public class UpdateUserDto
{
    [Required, StringLength(120)]
    public string FullName { get; set; } = string.Empty;

    [Required]
    public string Role { get; set; } = string.Empty;

    [Phone]
    public string? PhoneNumber { get; set; }

    [StringLength(250)]
    public string? Address { get; set; }

    public bool IsActive { get; set; } = true;

    [MinLength(8)]
    public string? Password { get; set; }
}
