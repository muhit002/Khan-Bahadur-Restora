using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RestaurantManagement.Api.DTOs.Auth;
using RestaurantManagement.Api.Helpers;
using RestaurantManagement.Api.Interfaces.Services;

namespace RestaurantManagement.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[AppAuthorize]
public class AuthController(IAuthService authService) : ControllerBase
{
    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<IActionResult> Register([FromBody] RegisterRequestDto request)
    {
        return Ok(await authService.RegisterAsync(request));
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
    {
        return Ok(await authService.LoginAsync(request));
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        return Ok(await authService.LogoutAsync());
    }

    [HttpGet("me")]
    public async Task<IActionResult> Me()
    {
        var userId = User.GetUserId() ?? throw new UnauthorizedAccessException("Invalid user context.");
        var user = await authService.GetCurrentUserAsync(userId);
        return user is null ? NotFound() : Ok(user);
    }
}
