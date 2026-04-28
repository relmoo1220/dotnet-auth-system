using auth_service.Data;
using auth_service.Modules.Auth.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace auth_service.Modules.Auth.Services;

public interface IAuthService
{
    Task RegisterAsync(User user, string plainTextPassword);
    Task<(string accessToken, string refreshToken)?> LoginAsync(
        string username,
        string plainTextPassword
    );
    Task LogoutAsync(string refreshToken);
    Task LogoutAllAsync(int userId);
}

public class AuthService : IAuthService
{
    private readonly AppDbContext _context;
    private readonly IPasswordHasher<User> _passwordHasher;
    private readonly IJwtService _jwtService;
    private readonly IRefreshTokenService _refreshTokenService;

    public AuthService(
        AppDbContext context,
        IPasswordHasher<User> passwordHasher,
        IJwtService jwtService,
        IRefreshTokenService refreshTokenService
    )
    {
        _context = context;
        _passwordHasher = passwordHasher;
        _jwtService = jwtService;
        _refreshTokenService = refreshTokenService;
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

    public async Task<(string accessToken, string refreshToken)?> LoginAsync(
        string username,
        string plainTextPassword
    )
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);

        if (user == null)
            return null;

        var result = _passwordHasher.VerifyHashedPassword(user, user.Password, plainTextPassword);

        if (result != PasswordVerificationResult.Success)
            return null;

        var accessToken = _jwtService.GenerateToken(user);
        var refreshToken = await _refreshTokenService.GenerateRefreshTokenAsync(user);
        return (accessToken, refreshToken);
    }

    public async Task LogoutAsync(string refreshToken)
    {
        var stored = await _context.RefreshTokens.FirstOrDefaultAsync(r => r.Token == refreshToken);

        if (stored == null)
            throw new Exception("Refresh token not found in DB");

        stored.IsRevoked = true;

        await _context.SaveChangesAsync();
    }

    public async Task LogoutAllAsync(int userId)
    {
        var tokens = await _context.RefreshTokens.Where(r => r.UserId == userId).ToListAsync();

        foreach (var t in tokens)
            t.IsRevoked = true;

        await _context.SaveChangesAsync();
    }
}
