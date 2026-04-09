using RestaurantManagement.Api.DTOs.Orders;

namespace RestaurantManagement.Api.DTOs.Dashboard;

public record SalesPointDto(string Label, decimal Amount);

public record TopMenuItemDto(string Name, int QuantitySold, decimal Revenue);

public class AdminDashboardDto
{
    public int TotalOrders { get; set; }
    public decimal TotalRevenue { get; set; }
    public int TotalCustomers { get; set; }
    public int TotalEmployees { get; set; }
    public int LowStockItems { get; set; }
    public int ActiveMenuItems { get; set; }
    public IReadOnlyCollection<TopMenuItemDto> TopMenuItems { get; set; } = [];
    public IReadOnlyCollection<SalesPointDto> SalesOverview { get; set; } = [];
}

public class ManagerDashboardDto
{
    public int TodayOrders { get; set; }
    public decimal TodayRevenue { get; set; }
    public int OpenOrders { get; set; }
    public IReadOnlyCollection<SalesPointDto> WeeklySales { get; set; } = [];
    public IReadOnlyCollection<TopMenuItemDto> TopItems { get; set; } = [];
}

public class CashierDashboardDto
{
    public int PendingPayments { get; set; }
    public decimal PendingAmount { get; set; }
    public int OpenOrders { get; set; }
    public IReadOnlyCollection<OrderListItemDto> RecentOrders { get; set; } = [];
}

public class ChefDashboardDto
{
    public int PendingOrders { get; set; }
    public int CookingOrders { get; set; }
    public int ReadyOrders { get; set; }
    public IReadOnlyCollection<KitchenOrderDto> KitchenQueue { get; set; } = [];
}

public class WaiterDashboardDto
{
    public int AssignedOrders { get; set; }
    public int ReadyOrders { get; set; }
    public int ServedToday { get; set; }
    public IReadOnlyCollection<KitchenOrderDto> ActiveOrders { get; set; } = [];
}

public class CustomerDashboardDto
{
    public int TotalOrders { get; set; }
    public decimal TotalSpent { get; set; }
    public int ActiveOrders { get; set; }
    public IReadOnlyCollection<OrderListItemDto> RecentOrders { get; set; } = [];
}
