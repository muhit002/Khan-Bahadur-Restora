using Microsoft.AspNetCore.Mvc;
using RestaurantManagement.Api.Common.Enums;
using RestaurantManagement.Api.Helpers;
using RestaurantManagement.Api.Interfaces.Services;

namespace RestaurantManagement.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[AppAuthorize(Roles = $"{AppRoles.Admin},{AppRoles.Manager}")]
public class UploadsController(IFileStorageService fileStorageService) : ControllerBase
{
    [HttpPost("image")]
    [RequestSizeLimit(10_000_000)]
    public async Task<IActionResult> UploadImage(IFormFile file, CancellationToken cancellationToken)
    {
        var path = await fileStorageService.SaveImageAsync(file, cancellationToken);
        return Ok(new { path });
    }
}
