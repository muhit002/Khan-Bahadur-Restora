using RestaurantManagement.Api.DTOs.Dashboard;

namespace RestaurantManagement.Api.Interfaces.Services;

public interface IDashboardService
{
    Task<AdminDashboardDto> GetAdminDashboardAsync();
    Task<ManagerDashboardDto> GetManagerDashboardAsync();
    Task<CashierDashboardDto> GetCashierDashboardAsync();
    Task<ChefDashboardDto> GetChefDashboardAsync(Guid? currentUserId);
    Task<WaiterDashboardDto> GetWaiterDashboardAsync(Guid? currentUserId);
    Task<CustomerDashboardDto> GetCustomerDashboardAsync(Guid currentUserId);
}
