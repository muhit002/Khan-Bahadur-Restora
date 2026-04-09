using Microsoft.AspNetCore.Mvc;
using RestaurantManagement.Api.Common.Enums;
using RestaurantManagement.Api.DTOs.Common;
using RestaurantManagement.Api.DTOs.Inventory;
using RestaurantManagement.Api.Helpers;
using RestaurantManagement.Api.Interfaces.Services;

namespace RestaurantManagement.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[AppAuthorize(Roles = $"{AppRoles.Admin},{AppRoles.Manager}")]
public class InventoryController(IInventoryService inventoryService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetInventory([FromQuery] QueryParameters query)
    {
        return Ok(await inventoryService.GetInventoryAsync(query));
    }

    [HttpGet("low-stock")]
    public async Task<IActionResult> GetLowStock()
    {
        return Ok(await inventoryService.GetLowStockAlertsAsync());
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var item = await inventoryService.GetByIdAsync(id);
        return item is null ? NotFound() : Ok(item);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateInventoryItemDto request)
    {
        var item = await inventoryService.CreateAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = item.Id }, item);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateInventoryItemDto request)
    {
        return Ok(await inventoryService.UpdateAsync(id, request));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await inventoryService.DeleteAsync(id);
        return NoContent();
    }
}
