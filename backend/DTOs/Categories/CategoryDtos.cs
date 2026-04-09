using System.ComponentModel.DataAnnotations;

namespace RestaurantManagement.Api.DTOs.Categories;

public record CategoryDto(
    Guid Id,
    string Name,
    string? Description,
    string? ImageUrl,
    bool IsActive,
    int SortOrder,
    int MenuItemsCount);

public class CreateCategoryDto
{
    [Required, StringLength(120)]
    public string Name { get; set; } = string.Empty;

    [StringLength(250)]
    public string? Description { get; set; }

    public string? ImageUrl { get; set; }

    public bool IsActive { get; set; } = true;

    public int SortOrder { get; set; }
}

public class UpdateCategoryDto : CreateCategoryDto;
