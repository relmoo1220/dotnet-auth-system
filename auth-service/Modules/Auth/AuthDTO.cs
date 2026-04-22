using System.ComponentModel.DataAnnotations;

namespace auth_service.Modules.Auth.DTOs;

public class RegisterRequest
{
    [Required]
    [MinLength(3)]
    public required string Username { get; set; }

    [Required]
    [MinLength(6)]
    public required string Password { get; set; }
}

public class LoginRequest
{
    [Required]
    public required string Username { get; set; }

    [Required]
    public required string Password { get; set; }
}

public class LogoutRequest
{
    public required string RefreshToken { get; set; }
}
