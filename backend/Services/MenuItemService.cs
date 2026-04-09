using Microsoft.EntityFrameworkCore;
using RestaurantManagement.Api.Data;
using RestaurantManagement.Api.DTOs.Common;
using RestaurantManagement.Api.DTOs.MenuItems;
using RestaurantManagement.Api.Entities;
using RestaurantManagement.Api.Helpers;
using RestaurantManagement.Api.Interfaces.Services;

namespace RestaurantManagement.Api.Services;

public class MenuItemService(ApplicationDbContext dbContext) : IMenuItemService
{
    public async Task<PagedResult<MenuItemDto>> GetMenuItemsAsync(QueryParameters query, Guid? categoryId, bool? isAvailable)
    {
        var menuItemsQuery = dbContext.MenuItems
            .AsNoTracking()
            .Include(x => x.Category)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.Trim().ToLower();
            menuItemsQuery = menuItemsQuery.Where(x =>
                x.Name.ToLower().Contains(search) ||
                (x.Description != null && x.Description.ToLower().Contains(search)));
        }

        if (categoryId.HasValue)
        {
            menuItemsQuery = menuItemsQuery.Where(x => x.CategoryId == categoryId.Value);
        }

        if (isAvailable.HasValue)
        {
            menuItemsQuery = menuItemsQuery.Where(x => x.IsAvailable == isAvailable.Value);
        }

        return await menuItemsQuery
            .OrderBy(x => x.Category!.SortOrder)
            .ThenBy(x => x.Name)
            .Select(x => x.ToDto())
            .ToPagedResultAsync(query.PageNumber, query.PageSize);
    }

    public async Task<MenuItemDto?> GetByIdAsync(Guid id)
    {
        var menuItem = await dbContext.MenuItems.AsNoTracking()
            .Include(x => x.Category)
            .FirstOrDefaultAsync(x => x.Id == id);
        return menuItem?.ToDto();
    }

    public async Task<MenuItemDto> CreateAsync(CreateMenuItemDto request)
    {
        var category = await dbContext.Categories.FirstOrDefaultAsync(x => x.Id == request.CategoryId) ??
            throw new KeyNotFoundException("Category not found.");

        var menuItem = new MenuItem
        {
            CategoryId = request.CategoryId,
            Name = request.Name.Trim(),
            Description = request.Description,
            Price = request.Price,
            DiscountPercentage = request.DiscountPercentage,
            IsAvailable = request.IsAvailable,
            ImageUrl = request.ImageUrl,
            PreparationTimeMinutes = request.PreparationTimeMinutes,
            Category = category
        };

        await dbContext.MenuItems.AddAsync(menuItem);
        await dbContext.SaveChangesAsync();
        return menuItem.ToDto();
    }

    public async Task<MenuItemDto> UpdateAsync(Guid id, UpdateMenuItemDto request)
    {
        var menuItem = await dbContext.MenuItems.Include(x => x.Category).FirstOrDefaultAsync(x => x.Id == id) ??
            throw new KeyNotFoundException("Menu item not found.");

        var category = await dbContext.Categories.FirstOrDefaultAsync(x => x.Id == request.CategoryId) ??
            throw new KeyNotFoundException("Category not found.");

        menuItem.CategoryId = category.Id;
        menuItem.Category = category;
        menuItem.Name = request.Name.Trim();
        menuItem.Description = request.Description;
        menuItem.Price = request.Price;
        menuItem.DiscountPercentage = request.DiscountPercentage;
        menuItem.IsAvailable = request.IsAvailable;
        menuItem.ImageUrl = request.ImageUrl;
        menuItem.PreparationTimeMinutes = request.PreparationTimeMinutes;

        await dbContext.SaveChangesAsync();
        return menuItem.ToDto();
    }

    public async Task DeleteAsync(Guid id)
    {
        var menuItem = await dbContext.MenuItems.FirstOrDefaultAsync(x => x.Id == id) ??
            throw new KeyNotFoundException("Menu item not found.");

        dbContext.MenuItems.Remove(menuItem);
        await dbContext.SaveChangesAsync();
    }
}
