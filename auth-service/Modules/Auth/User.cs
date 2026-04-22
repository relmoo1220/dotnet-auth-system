namespace auth_service.Modules.Auth.Models;

public class User
{
    public int Id { get; set; }
    public string Role { get; set; } = "user";
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}
