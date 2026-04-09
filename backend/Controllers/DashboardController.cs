using Microsoft.AspNetCore.Mvc;
using RestaurantManagement.Api.Common.Enums;
using RestaurantManagement.Api.Helpers;
using RestaurantManagement.Api.Interfaces.Services;

namespace RestaurantManagement.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[AppAuthorize]
public class DashboardController(IDashboardService dashboardService) : ControllerBase
{
    [HttpGet("admin")]
    [AppAuthorize(Roles = AppRoles.Admin)]
    public async Task<IActionResult> Admin()
    {
        return Ok(await dashboardService.GetAdminDashboardAsync());
    }

    [HttpGet("manager")]
    [AppAuthorize(Roles = $"{AppRoles.Admin},{AppRoles.Manager}")]
    public async Task<IActionResult> Manager()
    {
        return Ok(await dashboardService.GetManagerDashboardAsync());
    }

    [HttpGet("cashier")]
    [AppAuthorize(Roles = $"{AppRoles.Admin},{AppRoles.Manager},{AppRoles.Cashier}")]
    public async Task<IActionResult> Cashier()
    {
        return Ok(await dashboardService.GetCashierDashboardAsync());
    }

    [HttpGet("chef")]
    [AppAuthorize(Roles = $"{AppRoles.Admin},{AppRoles.Manager},{AppRoles.Chef}")]
    public async Task<IActionResult> Chef()
    {
        return Ok(await dashboardService.GetChefDashboardAsync(User.GetUserId()));
    }

    [HttpGet("waiter")]
    [AppAuthorize(Roles = $"{AppRoles.Admin},{AppRoles.Manager},{AppRoles.Waiter}")]
    public async Task<IActionResult> Waiter()
    {
        return Ok(await dashboardService.GetWaiterDashboardAsync(User.GetUserId()));
    }

    [HttpGet("customer")]
    [AppAuthorize(Roles = AppRoles.Customer)]
    public async Task<IActionResult> Customer()
    {
        var userId = User.GetUserId() ?? throw new UnauthorizedAccessException("Invalid user context.");
        return Ok(await dashboardService.GetCustomerDashboardAsync(userId));
    }
}
