using Microsoft.EntityFrameworkCore;
using RestaurantManagement.Api.Common.Enums;
using RestaurantManagement.Api.Data;
using RestaurantManagement.Api.DTOs.Dashboard;
using RestaurantManagement.Api.Interfaces.Services;

namespace RestaurantManagement.Api.Services;

public class ReportService(ApplicationDbContext dbContext) : IReportService
{
    public async Task<SalesSummaryReportDto> GetSalesSummaryAsync(string period)
    {
        var normalizedPeriod = string.IsNullOrWhiteSpace(period) ? "daily" : period.Trim().ToLowerInvariant();
        DateTime startDate;
        IReadOnlyCollection<SalesPointDto> trend;

        switch (normalizedPeriod)
        {
            case "weekly":
                startDate = DateTime.UtcNow.Date.AddDays(-27);
                trend = await GetTrendAsync(startDate, 4, date => $"Week {((date - startDate).Days / 7) + 1}", 7);
                break;
            case "monthly":
                startDate = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1, 0, 0, 0, DateTimeKind.Utc).AddMonths(-5);
                trend = await GetMonthlyTrendAsync(startDate, 6);
                break;
            default:
                normalizedPeriod = "daily";
                startDate = DateTime.UtcNow.Date.AddDays(-6);
                trend = await GetTrendAsync(startDate, 7, date => date.ToString("MMM dd"), 1);
                break;
        }

        var paidOrdersQuery = dbContext.Orders.Where(x => x.Status == OrderStatuses.Paid && x.CreatedAtUtc >= startDate);
        var totalRevenue = await paidOrdersQuery.SumAsync(x => (decimal?)x.TotalAmount) ?? 0m;
        var totalOrders = await paidOrdersQuery.CountAsync();

        return new SalesSummaryReportDto
        {
            Period = normalizedPeriod,
            TotalRevenue = totalRevenue,
            TotalOrders = totalOrders,
            AverageOrderValue = totalOrders == 0 ? 0m : Math.Round(totalRevenue / totalOrders, 2),
            Trend = trend
        };
    }

    public async Task<IReadOnlyCollection<TopMenuItemDto>> GetTopMenuItemsAsync(int take)
    {
        take = Math.Clamp(take, 1, 20);
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

    public async Task<IReadOnlyCollection<EmployeePerformanceDto>> GetEmployeePerformanceAsync()
    {
        var orderMetrics = await dbContext.Orders.AsNoTracking()
            .Where(x => x.WaiterId.HasValue)
            .GroupBy(x => x.WaiterId!.Value)
            .Select(group => new
            {
                UserId = group.Key,
                OrdersHandled = group.Count(),
                RevenueInfluenced = group.Sum(x => x.TotalAmount)
            })
            .ToDictionaryAsync(x => x.UserId, x => new { x.OrdersHandled, x.RevenueInfluenced });

        var employees = await dbContext.Users.AsNoTracking()
            .Where(x => x.Role != AppRoles.Customer)
            .OrderBy(x => x.FullName)
            .ToListAsync();

        return employees
            .Select(user =>
            {
                var metrics = orderMetrics.GetValueOrDefault(user.Id);
                return new EmployeePerformanceDto(
                    user.FullName,
                    user.Role,
                    metrics?.OrdersHandled ?? 0,
                    metrics?.RevenueInfluenced ?? 0m);
            })
            .OrderByDescending(x => x.OrdersHandled)
            .ThenBy(x => x.EmployeeName)
            .ToList();
    }

    private async Task<IReadOnlyCollection<SalesPointDto>> GetTrendAsync(DateTime startDate, int buckets, Func<DateTime, string> labelSelector, int daysPerBucket)
    {
        var payments = await dbContext.Payments.AsNoTracking()
            .Where(x => x.PaidAtUtc >= startDate && x.Status == PaymentStatuses.Completed)
            .ToListAsync();

        return Enumerable.Range(0, buckets)
            .Select(index =>
            {
                var bucketStart = startDate.AddDays(index * daysPerBucket);
                var bucketEnd = bucketStart.AddDays(daysPerBucket);
                var amount = payments.Where(x => x.PaidAtUtc >= bucketStart && x.PaidAtUtc < bucketEnd).Sum(x => x.Amount);
                return new SalesPointDto(labelSelector(bucketStart), amount);
            })
            .ToArray();
    }

    private async Task<IReadOnlyCollection<SalesPointDto>> GetMonthlyTrendAsync(DateTime startDate, int months)
    {
        var payments = await dbContext.Payments.AsNoTracking()
            .Where(x => x.PaidAtUtc >= startDate && x.Status == PaymentStatuses.Completed)
            .ToListAsync();

        return Enumerable.Range(0, months)
            .Select(index =>
            {
                var monthStart = startDate.AddMonths(index);
                var monthEnd = monthStart.AddMonths(1);
                var amount = payments.Where(x => x.PaidAtUtc >= monthStart && x.PaidAtUtc < monthEnd).Sum(x => x.Amount);
                return new SalesPointDto(monthStart.ToString("MMM yyyy"), amount);
            })
            .ToArray();
    }
}
