using auth_service.Data;
using auth_service.Modules.Auth.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace auth_service.Modules.Auth.Services;

public interface IAuthService
{
    Task RegisterAsync(User user, string plainTextPassword);
    Task<string?> LoginAsync(string username, string plainTextPassword);
}

public class AuthService : IAuthService
{
    private readonly AppDbContext _context;
    private readonly IPasswordHasher<User> _passwordHasher;
    private readonly IJwtService _jwtService;

    public AuthService(
        AppDbContext context,
        IPasswordHasher<User> passwordHasher,
        IJwtService jwtService
    )
    {
        _context = context;
        _passwordHasher = passwordHasher;
        _jwtService = jwtService;
    }

    public async Task RegisterAsync(User user, string plainTextPassword)
    {
        var exists = await _context.Users.AnyAsync(u => u.Username == user.Username);

        if (exists)
        {
            throw new Exception("Username already exists");
        }
        user.Password = _passwordHasher.HashPassword(user, plainTextPassword);

        _context.Users.Add(user);
        await _context.SaveChangesAsync();
    }

    public async Task<string?> LoginAsync(string username, string plainTextPassword)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);

        if (user == null)
            return null;

        var result = _passwordHasher.VerifyHashedPassword(user, user.Password, plainTextPassword);

        if (result != PasswordVerificationResult.Success)
            return null;

        return _jwtService.GenerateToken(user);
    }
}
