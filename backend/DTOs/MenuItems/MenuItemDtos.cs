using System.ComponentModel.DataAnnotations;

namespace RestaurantManagement.Api.DTOs.MenuItems;

public record MenuItemDto(
    Guid Id,
    Guid CategoryId,
    string CategoryName,
    string Name,
    string? Description,
    decimal Price,
    decimal DiscountPercentage,
    decimal FinalPrice,
    bool IsAvailable,
    string? ImageUrl,
    int PreparationTimeMinutes);

public class CreateMenuItemDto
{
    [Required]
    public Guid CategoryId { get; set; }

    [Required, StringLength(150)]
    public string Name { get; set; } = string.Empty;

    [StringLength(500)]
    public string? Description { get; set; }

    [Range(0.01, 100000)]
    public decimal Price { get; set; }

    [Range(0, 100)]
    public decimal DiscountPercentage { get; set; }

    public bool IsAvailable { get; set; } = true;

    public string? ImageUrl { get; set; }

    [Range(1, 180)]
    public int PreparationTimeMinutes { get; set; } = 15;
}

public class UpdateMenuItemDto : CreateMenuItemDto;
