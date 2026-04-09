using RestaurantManagement.Api.DTOs.Dashboard;

namespace RestaurantManagement.Api.Interfaces.Services;

public interface IReportService
{
    Task<SalesSummaryReportDto> GetSalesSummaryAsync(string period);
    Task<IReadOnlyCollection<TopMenuItemDto>> GetTopMenuItemsAsync(int take);
    Task<IReadOnlyCollection<EmployeePerformanceDto>> GetEmployeePerformanceAsync();
}
