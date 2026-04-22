using System.Security.Cryptography;
using auth_service.Data;
using auth_service.Modules.Auth.Models;
using Microsoft.EntityFrameworkCore;

namespace auth_service.Modules.Auth.Services;

public interface IRefreshTokenService
{
    Task<string> GenerateRefreshTokenAsync(User user);
    Task<(string accessToken, string refreshToken)> RefreshAsync(string refreshToken);
}

public class RefreshTokenService : IRefreshTokenService
{
    private readonly AppDbContext _context;
    private readonly IJwtService _jwtService;

    public RefreshTokenService(AppDbContext context, IJwtService jwtService)
    {
        _context = context;
        _jwtService = jwtService;
    }

    public async Task<string> GenerateRefreshTokenAsync(User user)
    {
        var token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));

        var refreshToken = new RefreshToken
        {
            Token = token,
            UserId = user.Id,
            ExpiryDate = DateTime.UtcNow.AddDays(7),
            IsRevoked = false,
        };

        _context.RefreshTokens.Add(refreshToken);
        await _context.SaveChangesAsync();

        return token;
    }

    public async Task<(string accessToken, string refreshToken)> RefreshAsync(string token)
    {
        var stored = await _context
            .RefreshTokens.Include(r => r.User)
            .FirstOrDefaultAsync(r => r.Token == token);

        if (stored == null || stored.IsRevoked || stored.ExpiryDate < DateTime.UtcNow)
            throw new Exception("Invalid refresh token");

        // rotate token (important)
        stored.IsRevoked = true;

        var newAccessToken = _jwtService.GenerateToken(stored.User);
        var newRefreshToken = await GenerateRefreshTokenAsync(stored.User);

        await _context.SaveChangesAsync();

        return (newAccessToken, newRefreshToken);
    }
}
