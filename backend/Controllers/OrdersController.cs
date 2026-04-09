using Microsoft.AspNetCore.Mvc;
using RestaurantManagement.Api.Common.Enums;
using RestaurantManagement.Api.DTOs.Common;
using RestaurantManagement.Api.DTOs.Orders;
using RestaurantManagement.Api.Helpers;
using RestaurantManagement.Api.Interfaces.Services;

namespace RestaurantManagement.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[AppAuthorize]
public class OrdersController(IOrderService orderService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetOrders([FromQuery] QueryParameters query, [FromQuery] string? status)
    {
        var currentRole = User.GetRoleName();
        var currentUserId = User.GetUserId();
        var customerScope = currentRole == AppRoles.Customer ? currentUserId : null;
        var waiterScope = currentRole == AppRoles.Waiter ? currentUserId : null;
        return Ok(await orderService.GetOrdersAsync(query, status, customerScope, waiterScope));
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var order = await orderService.GetByIdAsync(id);
        if (order is null)
        {
            return NotFound();
        }

        var currentRole = User.GetRoleName();
        var currentUserId = User.GetUserId();
        if (currentRole == AppRoles.Customer && order.CustomerId != currentUserId)
        {
            return Forbid();
        }

        if (currentRole == AppRoles.Waiter && order.WaiterId != currentUserId)
        {
            return Forbid();
        }

        return Ok(order);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateOrderDto request)
    {
        var order = await orderService.CreateAsync(request, User.GetUserId(), User.GetRoleName());
        return CreatedAtAction(nameof(GetById), new { id = order.Id }, order);
    }

    [HttpPut("{id:guid}/status")]
    [AppAuthorize(Roles = $"{AppRoles.Admin},{AppRoles.Manager},{AppRoles.Chef},{AppRoles.Waiter},{AppRoles.Cashier}")]
    public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] UpdateOrderStatusDto request)
    {
        return Ok(await orderService.UpdateStatusAsync(id, request));
    }

    [HttpGet("{id:guid}/invoice")]
    public async Task<IActionResult> GetInvoice(Guid id)
    {
        var currentRole = User.GetRoleName();
        var currentUserId = User.GetUserId();
        var order = await orderService.GetByIdAsync(id);
        if (order is null)
        {
            return NotFound();
        }

        if (currentRole == AppRoles.Customer && order.CustomerId != currentUserId)
        {
            return Forbid();
        }

        var invoice = await orderService.GetInvoiceAsync(id);
        return invoice is null ? NotFound() : Ok(invoice);
    }
}
