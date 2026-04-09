using RestaurantManagement.Api.DTOs.Tables;

namespace RestaurantManagement.Api.Interfaces.Services;

public interface ITableService
{
    Task<IReadOnlyCollection<TableDto>> GetTablesAsync();
    Task<IReadOnlyCollection<TableDto>> GetAvailableTablesAsync(int guests);
    Task<TableDto?> GetByIdAsync(Guid id);
    Task<TableDto> CreateAsync(CreateTableDto request);
    Task<TableDto> UpdateAsync(Guid id, UpdateTableDto request);
    Task DeleteAsync(Guid id);
}
