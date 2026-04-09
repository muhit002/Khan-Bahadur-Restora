namespace RestaurantManagement.Api.DTOs.Dashboard;

public class SalesSummaryReportDto
{
    public string Period { get; set; } = string.Empty;
    public decimal TotalRevenue { get; set; }
    public int TotalOrders { get; set; }
    public decimal AverageOrderValue { get; set; }
    public IReadOnlyCollection<SalesPointDto> Trend { get; set; } = [];
}

public record EmployeePerformanceDto(string EmployeeName, string Role, int OrdersHandled, decimal RevenueInfluenced);
