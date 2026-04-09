using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RestaurantManagement.Api.Common.Enums;
using RestaurantManagement.Api.DTOs.Categories;
using RestaurantManagement.Api.DTOs.Common;
using RestaurantManagement.Api.Helpers;
using RestaurantManagement.Api.Interfaces.Services;

namespace RestaurantManagement.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[AppAuthorize]
public class CategoriesController(ICategoryService categoryService) : ControllerBase
{
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetCategories([FromQuery] QueryParameters query)
    {
        return Ok(await categoryService.GetCategoriesAsync(query));
    }

    [HttpGet("active")]
    [AllowAnonymous]
    public async Task<IActionResult> GetActive()
    {
        return Ok(await categoryService.GetActiveCategoriesAsync());
    }

    [HttpGet("{id:guid}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetById(Guid id)
    {
        var category = await categoryService.GetByIdAsync(id);
        return category is null ? NotFound() : Ok(category);
    }

    [HttpPost]
    [AppAuthorize(Roles = $"{AppRoles.Admin},{AppRoles.Manager}")]
    public async Task<IActionResult> Create([FromBody] CreateCategoryDto request)
    {
        var category = await categoryService.CreateAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = category.Id }, category);
    }

    [HttpPut("{id:guid}")]
    [AppAuthorize(Roles = $"{AppRoles.Admin},{AppRoles.Manager}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateCategoryDto request)
    {
        return Ok(await categoryService.UpdateAsync(id, request));
    }

    [HttpDelete("{id:guid}")]
    [AppAuthorize(Roles = $"{AppRoles.Admin},{AppRoles.Manager}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await categoryService.DeleteAsync(id);
        return NoContent();
    }
}
