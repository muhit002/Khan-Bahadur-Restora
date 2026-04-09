using Microsoft.EntityFrameworkCore;
using RestaurantManagement.Api.Data;
using RestaurantManagement.Api.DTOs.Categories;
using RestaurantManagement.Api.DTOs.Common;
using RestaurantManagement.Api.Entities;
using RestaurantManagement.Api.Helpers;
using RestaurantManagement.Api.Interfaces.Services;

namespace RestaurantManagement.Api.Services;

public class CategoryService(ApplicationDbContext dbContext) : ICategoryService
{
    public async Task<PagedResult<CategoryDto>> GetCategoriesAsync(QueryParameters query)
    {
        var categoriesQuery = dbContext.Categories
            .AsNoTracking()
            .Include(x => x.MenuItems)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.Trim().ToLower();
            categoriesQuery = categoriesQuery.Where(x => x.Name.ToLower().Contains(search));
        }

        return await categoriesQuery
            .OrderBy(x => x.SortOrder)
            .ThenBy(x => x.Name)
            .Select(x => x.ToDto())
            .ToPagedResultAsync(query.PageNumber, query.PageSize);
    }

    public async Task<IReadOnlyCollection<CategoryDto>> GetActiveCategoriesAsync()
    {
        return await dbContext.Categories.AsNoTracking()
            .Include(x => x.MenuItems)
            .Where(x => x.IsActive)
            .OrderBy(x => x.SortOrder)
            .ThenBy(x => x.Name)
            .Select(x => x.ToDto())
            .ToListAsync();
    }

    public async Task<CategoryDto?> GetByIdAsync(Guid id)
    {
        var category = await dbContext.Categories.AsNoTracking()
            .Include(x => x.MenuItems)
            .FirstOrDefaultAsync(x => x.Id == id);
        return category?.ToDto();
    }

    public async Task<CategoryDto> CreateAsync(CreateCategoryDto request)
    {
        var exists = await dbContext.Categories.AnyAsync(x => x.Name == request.Name.Trim());
        if (exists)
        {
            throw new InvalidOperationException("A category with this name already exists.");
        }

        var category = new Category
        {
            Name = request.Name.Trim(),
            Description = request.Description,
            ImageUrl = request.ImageUrl,
            IsActive = request.IsActive,
            SortOrder = request.SortOrder
        };

        await dbContext.Categories.AddAsync(category);
        await dbContext.SaveChangesAsync();
        return category.ToDto();
    }

    public async Task<CategoryDto> UpdateAsync(Guid id, UpdateCategoryDto request)
    {
        var category = await dbContext.Categories.Include(x => x.MenuItems).FirstOrDefaultAsync(x => x.Id == id) ??
            throw new KeyNotFoundException("Category not found.");

        var duplicateExists = await dbContext.Categories.AnyAsync(x => x.Id != id && x.Name == request.Name.Trim());
        if (duplicateExists)
        {
            throw new InvalidOperationException("Another category already uses this name.");
        }

        category.Name = request.Name.Trim();
        category.Description = request.Description;
        category.ImageUrl = request.ImageUrl;
        category.IsActive = request.IsActive;
        category.SortOrder = request.SortOrder;

        await dbContext.SaveChangesAsync();
        return category.ToDto();
    }

    public async Task DeleteAsync(Guid id)
    {
        var category = await dbContext.Categories.Include(x => x.MenuItems).FirstOrDefaultAsync(x => x.Id == id) ??
            throw new KeyNotFoundException("Category not found.");

        if (category.MenuItems.Any())
        {
            throw new InvalidOperationException("Delete or move menu items before removing this category.");
        }

        dbContext.Categories.Remove(category);
        await dbContext.SaveChangesAsync();
    }
}
