using RestaurantManagement.Api.DTOs.Common;
using RestaurantManagement.Api.DTOs.MenuItems;

namespace RestaurantManagement.Api.Interfaces.Services;

public interface IMenuItemService
{
    Task<PagedResult<MenuItemDto>> GetMenuItemsAsync(QueryParameters query, Guid? categoryId, bool? isAvailable);
    Task<MenuItemDto?> GetByIdAsync(Guid id);
    Task<MenuItemDto> CreateAsync(CreateMenuItemDto request);
    Task<MenuItemDto> UpdateAsync(Guid id, UpdateMenuItemDto request);
    Task DeleteAsync(Guid id);
}
