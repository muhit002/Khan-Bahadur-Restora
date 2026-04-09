using Microsoft.EntityFrameworkCore;
using RestaurantManagement.Api.Common.Enums;
using RestaurantManagement.Api.Data;
using RestaurantManagement.Api.DTOs.Dashboard;
using RestaurantManagement.Api.Helpers;
using RestaurantManagement.Api.Interfaces.Services;

namespace RestaurantManagement.Api.Services;

public class DashboardService(ApplicationDbContext dbContext) : IDashboardService
{
    public async Task<AdminDashboardDto> GetAdminDashboardAsync()
    {
        var totalOrders = await dbContext.Orders.CountAsync();
        var totalRevenue = await dbContext.Payments.Where(x => x.Status == PaymentStatuses.Completed).SumAsync(x => (decimal?)x.Amount) ?? 0m;
        var totalCustomers = await dbContext.Users.CountAsync(x => x.Role == AppRoles.Customer);
        var totalEmployees = await dbContext.Users.CountAsync(x => x.Role != AppRoles.Customer);
        var lowStockItems = await dbContext.InventoryItems.CountAsync(x => x.QuantityInStock <= x.ReorderLevel);
        var activeMenuItems = await dbContext.MenuItems.CountAsync(x => x.IsAvailable);

        return new AdminDashboardDto
        {
            TotalOrders = totalOrders,
            TotalRevenue = totalRevenue,
            TotalCustomers = totalCustomers,
            TotalEmployees = totalEmployees,
            LowStockItems = lowStockItems,
            ActiveMenuItems = activeMenuItems,
            TopMenuItems = await GetTopItemsAsync(5),
            SalesOverview = await GetRecentSalesAsync(7)
        };
    }

    public async Task<ManagerDashboardDto> GetManagerDashboardAsync()
    {
        var today = DateTime.UtcNow.Date;
        return new ManagerDashboardDto
        {
            TodayOrders = await dbContext.Orders.CountAsync(x => x.CreatedAtUtc.Date == today),
            TodayRevenue = await dbContext.Payments.Where(x => x.PaidAtUtc.Date == today && x.Status == PaymentStatuses.Completed).SumAsync(x => (decimal?)x.Amount) ?? 0m,
            OpenOrders = await dbContext.Orders.CountAsync(x => x.Status != OrderStatuses.Paid && x.Status != OrderStatuses.Cancelled),
            WeeklySales = await GetRecentSalesAsync(7),
            TopItems = await GetTopItemsAsync(5)
        };
    }

    public async Task<CashierDashboardDto> GetCashierDashboardAsync()
    {
        var openOrders = await dbContext.Orders
            .AsNoTracking()
            .Include(x => x.Customer)
            .Include(x => x.Waiter)
            .Include(x => x.Table)
            .Where(x => x.Status != OrderStatuses.Paid && x.Status != OrderStatuses.Cancelled)
            .OrderByDescending(x => x.CreatedAtUtc)
            .Take(8)
            .ToListAsync();

        var pendingAmount = await dbContext.Orders
            .Where(x => x.Status != OrderStatuses.Paid && x.Status != OrderStatuses.Cancelled)
            .SumAsync(x => (decimal?)x.TotalAmount) ?? 0m;

        return new CashierDashboardDto
        {
            PendingPayments = await dbContext.Orders.CountAsync(x => x.Status != OrderStatuses.Paid && x.Status != OrderStatuses.Cancelled),
            PendingAmount = pendingAmount,
            OpenOrders = openOrders.Count,
            RecentOrders = openOrders.Select(x => x.ToListDto()).ToArray()
        };
    }

    public async Task<ChefDashboardDto> GetChefDashboardAsync(Guid? currentUserId)
    {
        var kitchenOrders = await dbContext.Orders.AsNoTracking()
            .Include(x => x.Table)
            .Include(x => x.Items).ThenInclude(x => x.MenuItem)
            .Where(x => x.Status == OrderStatuses.Pending || x.Status == OrderStatuses.Cooking || x.Status == OrderStatuses.Ready)
            .OrderBy(x => x.CreatedAtUtc)
            .ToListAsync();

        return new ChefDashboardDto
        {
            PendingOrders = kitchenOrders.Count(x => x.Status == OrderStatuses.Pending),
            CookingOrders = kitchenOrders.Count(x => x.Status == OrderStatuses.Cooking),
            ReadyOrders = kitchenOrders.Count(x => x.Status == OrderStatuses.Ready),
            KitchenQueue = kitchenOrders.Select(x => x.ToKitchenDto()).ToArray()
        };
    }

    public async Task<WaiterDashboardDto> GetWaiterDashboardAsync(Guid? currentUserId)
    {
        var ordersQuery = dbContext.Orders.AsNoTracking()
            .Include(x => x.Table)
            .Include(x => x.Items).ThenInclude(x => x.MenuItem)
            .Where(x => x.WaiterId == currentUserId || currentUserId == null);

        var activeOrders = await ordersQuery
            .Where(x => x.Status != OrderStatuses.Cancelled && x.Status != OrderStatuses.Paid)
            .OrderByDescending(x => x.CreatedAtUtc)
            .ToListAsync();

        return new WaiterDashboardDto
        {
            AssignedOrders = activeOrders.Count,
            ReadyOrders = activeOrders.Count(x => x.Status == OrderStatuses.Ready),
            ServedToday = await dbContext.Orders.CountAsync(x =>
                (x.WaiterId == currentUserId || currentUserId == null) &&
                x.Status == OrderStatuses.Served &&
                x.CreatedAtUtc.Date == DateTime.UtcNow.Date),
            ActiveOrders = activeOrders.Select(x => x.ToKitchenDto()).ToArray()
        };
    }

    public async Task<CustomerDashboardDto> GetCustomerDashboardAsync(Guid currentUserId)
    {
        var orders = await dbContext.Orders.AsNoTracking()
            .Include(x => x.Customer)
            .Include(x => x.Waiter)
            .Include(x => x.Table)
            .Where(x => x.CustomerId == currentUserId)
            .OrderByDescending(x => x.CreatedAtUtc)
            .Take(5)
            .ToListAsync();

        return new CustomerDashboardDto
        {
            TotalOrders = await dbContext.Orders.CountAsync(x => x.CustomerId == currentUserId),
            TotalSpent = await dbContext.Payments.Where(x => x.Order!.CustomerId == currentUserId && x.Status == PaymentStatuses.Completed).SumAsync(x => (decimal?)x.Amount) ?? 0m,
            ActiveOrders = await dbContext.Orders.CountAsync(x => x.CustomerId == currentUserId && x.Status != OrderStatuses.Paid && x.Status != OrderStatuses.Cancelled),
            RecentOrders = orders.Select(x => x.ToListDto()).ToArray()
        };
    }

    private async Task<IReadOnlyCollection<TopMenuItemDto>> GetTopItemsAsync(int take)
    {
        var results = await dbContext.OrderItems.AsNoTracking()
            .Where(x => x.MenuItem != null)
            .GroupBy(x => new { x.MenuItemId, Name = x.MenuItem!.Name })
            .Select(group => new
            {
                group.Key.Name,
                QuantitySold = group.Sum(x => x.Quantity),
                Revenue = group.Sum(x => x.TotalPrice)
            })
            .OrderByDescending(x => x.QuantitySold)
            .Take(take)
            .ToListAsync();

        return results
            .Select(x => new TopMenuItemDto(x.Name, x.QuantitySold, x.Revenue))
            .ToArray();
    }

    private async Task<IReadOnlyCollection<SalesPointDto>> GetRecentSalesAsync(int days)
    {
        var startDate = DateTime.UtcNow.Date.AddDays(-(days - 1));
        var salesByDate = await dbContext.Payments.AsNoTracking()
            .Where(x => x.PaidAtUtc.Date >= startDate && x.Status == PaymentStatuses.Completed)
            .GroupBy(x => x.PaidAtUtc.Date)
            .Select(group => new { Date = group.Key, Amount = group.Sum(x => x.Amount) })
            .ToDictionaryAsync(x => x.Date, x => x.Amount);

        return Enumerable.Range(0, days)
            .Select(offset =>
            {
                var day = startDate.AddDays(offset);
                return new SalesPointDto(day.ToString("MMM dd"), salesByDate.GetValueOrDefault(day, 0m));
            })
            .ToArray();
    }
}
