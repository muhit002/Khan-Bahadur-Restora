using Microsoft.EntityFrameworkCore;
using RestaurantManagement.Api.Data;
using RestaurantManagement.Api.DTOs.Common;
using RestaurantManagement.Api.DTOs.Inventory;
using RestaurantManagement.Api.Entities;
using RestaurantManagement.Api.Helpers;
using RestaurantManagement.Api.Interfaces.Services;

namespace RestaurantManagement.Api.Services;

public class InventoryService(ApplicationDbContext dbContext) : IInventoryService
{
    public async Task<PagedResult<InventoryItemDto>> GetInventoryAsync(QueryParameters query)
    {
        var inventoryQuery = dbContext.InventoryItems.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.Trim().ToLower();
            inventoryQuery = inventoryQuery.Where(x =>
                x.Name.ToLower().Contains(search) ||
                (x.SupplierName != null && x.SupplierName.ToLower().Contains(search)));
        }

        return await inventoryQuery
            .OrderBy(x => x.Name)
            .Select(x => x.ToDto())
            .ToPagedResultAsync(query.PageNumber, query.PageSize);
    }

    public async Task<IReadOnlyCollection<LowStockAlertDto>> GetLowStockAlertsAsync()
    {
        return await dbContext.InventoryItems.AsNoTracking()
            .Where(x => x.QuantityInStock <= x.ReorderLevel)
            .OrderBy(x => x.QuantityInStock)
            .Select(x => x.ToLowStockDto())
            .ToListAsync();
    }

    public async Task<InventoryItemDto?> GetByIdAsync(Guid id)
    {
        var item = await dbContext.InventoryItems.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
        return item?.ToDto();
    }

    public async Task<InventoryItemDto> CreateAsync(CreateInventoryItemDto request)
    {
        var item = new InventoryItem
        {
            Name = request.Name.Trim(),
            Unit = request.Unit.Trim(),
            QuantityInStock = request.QuantityInStock,
            ReorderLevel = request.ReorderLevel,
            CostPerUnit = request.CostPerUnit,
            SupplierName = request.SupplierName,
            Notes = request.Notes,
            LastUpdatedAtUtc = DateTime.UtcNow
        };

        await dbContext.InventoryItems.AddAsync(item);
        await dbContext.SaveChangesAsync();
        return item.ToDto();
    }

    public async Task<InventoryItemDto> UpdateAsync(Guid id, UpdateInventoryItemDto request)
    {
        var item = await dbContext.InventoryItems.FirstOrDefaultAsync(x => x.Id == id) ??
            throw new KeyNotFoundException("Inventory item not found.");

        item.Name = request.Name.Trim();
        item.Unit = request.Unit.Trim();
        item.QuantityInStock = request.QuantityInStock;
        item.ReorderLevel = request.ReorderLevel;
        item.CostPerUnit = request.CostPerUnit;
        item.SupplierName = request.SupplierName;
        item.Notes = request.Notes;
        item.LastUpdatedAtUtc = DateTime.UtcNow;

        await dbContext.SaveChangesAsync();
        return item.ToDto();
    }

    public async Task DeleteAsync(Guid id)
    {
        var item = await dbContext.InventoryItems.FirstOrDefaultAsync(x => x.Id == id) ??
            throw new KeyNotFoundException("Inventory item not found.");

        dbContext.InventoryItems.Remove(item);
        await dbContext.SaveChangesAsync();
    }
}
