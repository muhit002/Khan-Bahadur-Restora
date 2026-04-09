using Microsoft.AspNetCore.Mvc;
using RestaurantManagement.Api.Common.Enums;
using RestaurantManagement.Api.Helpers;
using RestaurantManagement.Api.Interfaces.Services;

namespace RestaurantManagement.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[AppAuthorize(Roles = $"{AppRoles.Admin},{AppRoles.Manager}")]
public class ReportsController(IReportService reportService) : ControllerBase
{
    [HttpGet("sales-summary")]
    public async Task<IActionResult> GetSalesSummary([FromQuery] string period = "daily")
    {
        return Ok(await reportService.GetSalesSummaryAsync(period));
    }

    [HttpGet("top-menu-items")]
    public async Task<IActionResult> GetTopMenuItems([FromQuery] int take = 5)
    {
        return Ok(await reportService.GetTopMenuItemsAsync(take));
    }

    [HttpGet("employee-performance")]
    public async Task<IActionResult> GetEmployeePerformance()
    {
        return Ok(await reportService.GetEmployeePerformanceAsync());
    }
}
