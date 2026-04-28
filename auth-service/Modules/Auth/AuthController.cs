using System.Security.Claims;
using auth_service.Modules.Auth.DTOs;
using auth_service.Modules.Auth.Models;
using auth_service.Modules.Auth.Services;
using Microsoft.AspNetCore.Mvc;

namespace auth_service.Modules.Auth.Controllers;

[ApiController]
[Route("auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly IRefreshTokenService _refreshTokenService;

    public AuthController(IAuthService authService, IRefreshTokenService refreshTokenService)
    {
        _authService = authService;
        _refreshTokenService = refreshTokenService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        var user = new User { Username = request.Username };

        await _authService.RegisterAsync(user, request.Password);

        return Ok(new { message = "User registered successfully" });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var result = await _authService.LoginAsync(request.Username, request.Password);

        if (result is null)
            return Unauthorized(new { message = "Invalid credentials" });

        var (accessToken, refreshToken) = result.Value;

        Response.Cookies.Append(
            "refreshToken",
            refreshToken,
            new CookieOptions
            {
                HttpOnly = true,
                Secure = false,
                SameSite = SameSiteMode.Strict,
                Path = "/",
                Expires = DateTime.UtcNow.AddDays(7),
            }
        );

        return Ok(new { accessToken });
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        var refreshToken = Request.Cookies["refreshToken"];

        if (string.IsNullOrEmpty(refreshToken))
        {
            Response.Cookies.Delete("refreshToken");
            return Unauthorized(new { message = "Refresh token missing" });
        }

        await _authService.LogoutAsync(refreshToken);

        Response.Cookies.Delete("refreshToken");

        return Ok(new { message = "Logged out successfully" });
    }

    [HttpPost("logout-all")]
    public async Task<IActionResult> LogoutAll()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(userIdClaim))
        {
            Response.Cookies.Delete("refreshToken");
            return Unauthorized(new { message = "User not authenticated" });
        }

        var userId = int.Parse(userIdClaim);

        await _authService.LogoutAllAsync(userId);

        Response.Cookies.Delete("refreshToken");

        return Ok(new { message = "Logged out from all sessions" });
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh()
    {
        var refreshToken = Request.Cookies["refreshToken"];

        if (string.IsNullOrEmpty(refreshToken))
            return Unauthorized();

        var result = await _refreshTokenService.RefreshAsync(refreshToken);

        Response.Cookies.Append(
            "refreshToken",
            result.refreshToken,
            new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Path = "/auth/refresh",
                Expires = DateTime.UtcNow.AddDays(7),
            }
        );

        return Ok(new { accessToken = result.accessToken });
    }
}
