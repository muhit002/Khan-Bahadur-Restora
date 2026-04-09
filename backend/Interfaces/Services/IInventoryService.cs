using RestaurantManagement.Api.DTOs.Common;
using RestaurantManagement.Api.DTOs.Inventory;

namespace RestaurantManagement.Api.Interfaces.Services;

public interface IInventoryService
{
    Task<PagedResult<InventoryItemDto>> GetInventoryAsync(QueryParameters query);
    Task<IReadOnlyCollection<LowStockAlertDto>> GetLowStockAlertsAsync();
    Task<InventoryItemDto?> GetByIdAsync(Guid id);
    Task<InventoryItemDto> CreateAsync(CreateInventoryItemDto request);
    Task<InventoryItemDto> UpdateAsync(Guid id, UpdateInventoryItemDto request);
    Task DeleteAsync(Guid id);
}
