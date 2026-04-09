using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RestaurantManagement.Api.Common.Enums;
using RestaurantManagement.Api.DTOs.Common;
using RestaurantManagement.Api.DTOs.MenuItems;
using RestaurantManagement.Api.Helpers;
using RestaurantManagement.Api.Interfaces.Services;

namespace RestaurantManagement.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[AppAuthorize]
public class MenuItemsController(IMenuItemService menuItemService) : ControllerBase
{
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetMenuItems([FromQuery] QueryParameters query, [FromQuery] Guid? categoryId, [FromQuery] bool? isAvailable)
    {
        return Ok(await menuItemService.GetMenuItemsAsync(query, categoryId, isAvailable));
    }

    [HttpGet("{id:guid}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetById(Guid id)
    {
        var menuItem = await menuItemService.GetByIdAsync(id);
        return menuItem is null ? NotFound() : Ok(menuItem);
    }

    [HttpPost]
    [AppAuthorize(Roles = $"{AppRoles.Admin},{AppRoles.Manager}")]
    public async Task<IActionResult> Create([FromBody] CreateMenuItemDto request)
    {
        var menuItem = await menuItemService.CreateAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = menuItem.Id }, menuItem);
    }

    [HttpPut("{id:guid}")]
    [AppAuthorize(Roles = $"{AppRoles.Admin},{AppRoles.Manager}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateMenuItemDto request)
    {
        return Ok(await menuItemService.UpdateAsync(id, request));
    }

    [HttpDelete("{id:guid}")]
    [AppAuthorize(Roles = $"{AppRoles.Admin},{AppRoles.Manager}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await menuItemService.DeleteAsync(id);
        return NoContent();
    }
}
