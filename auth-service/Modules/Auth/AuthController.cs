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

    public AuthController(IAuthService authService)
    {
        _authService = authService;
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
        var token = await _authService.LoginAsync(request.Username, request.Password);

        if (token == null)
        {
            return Unauthorized(new { message = "Invalid credentials" });
        }

        return Ok(new { token });
    }
}
