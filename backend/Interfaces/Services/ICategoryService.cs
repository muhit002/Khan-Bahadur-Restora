using RestaurantManagement.Api.DTOs.Categories;
using RestaurantManagement.Api.DTOs.Common;

namespace RestaurantManagement.Api.Interfaces.Services;

public interface ICategoryService
{
    Task<PagedResult<CategoryDto>> GetCategoriesAsync(QueryParameters query);
    Task<IReadOnlyCollection<CategoryDto>> GetActiveCategoriesAsync();
    Task<CategoryDto?> GetByIdAsync(Guid id);
    Task<CategoryDto> CreateAsync(CreateCategoryDto request);
    Task<CategoryDto> UpdateAsync(Guid id, UpdateCategoryDto request);
    Task DeleteAsync(Guid id);
}
