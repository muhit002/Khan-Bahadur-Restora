using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RestaurantManagement.Api.Common.Enums;
using RestaurantManagement.Api.DTOs.Tables;
using RestaurantManagement.Api.Helpers;
using RestaurantManagement.Api.Interfaces.Services;

namespace RestaurantManagement.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[AppAuthorize]
public class TablesController(ITableService tableService) : ControllerBase
{
    [HttpGet]
    [AppAuthorize(Roles = $"{AppRoles.Admin},{AppRoles.Manager},{AppRoles.Waiter},{AppRoles.Cashier}")]
    public async Task<IActionResult> GetTables()
    {
        return Ok(await tableService.GetTablesAsync());
    }

    [HttpGet("available")]
    [AllowAnonymous]
    public async Task<IActionResult> GetAvailableTables([FromQuery] int guests = 1)
    {
        return Ok(await tableService.GetAvailableTablesAsync(guests));
    }

    [HttpGet("{id:guid}")]
    [AppAuthorize(Roles = $"{AppRoles.Admin},{AppRoles.Manager},{AppRoles.Waiter},{AppRoles.Cashier}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var table = await tableService.GetByIdAsync(id);
        return table is null ? NotFound() : Ok(table);
    }

    [HttpPost]
    [AppAuthorize(Roles = $"{AppRoles.Admin},{AppRoles.Manager}")]
    public async Task<IActionResult> Create([FromBody] CreateTableDto request)
    {
        var table = await tableService.CreateAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = table.Id }, table);
    }

    [HttpPut("{id:guid}")]
    [AppAuthorize(Roles = $"{AppRoles.Admin},{AppRoles.Manager}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateTableDto request)
    {
        return Ok(await tableService.UpdateAsync(id, request));
    }

    [HttpDelete("{id:guid}")]
    [AppAuthorize(Roles = $"{AppRoles.Admin},{AppRoles.Manager}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await tableService.DeleteAsync(id);
        return NoContent();
    }
}
