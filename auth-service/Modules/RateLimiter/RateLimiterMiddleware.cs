using System.Security.Claims;

namespace auth_service.Modules.RateLimiter;

public class RateLimiterMiddleware
{
    private readonly RequestDelegate _next;

    public RateLimiterMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, RateLimiterService limiter)
    {
        var userId =
            context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? context.Connection.RemoteIpAddress?.ToString()
            ?? "anonymous";

        var key = $"rate_limit:{userId}";

        var (allowed, tokensLeft) = await limiter.AllowRequestAsync(
            key,
            capacity: 5,
            refillRate: 1,
            refillIntervalSeconds: 1
        );

        context.Response.Headers["X-RateLimit-Remaining"] = Math.Floor(tokensLeft).ToString();

        if (!allowed)
        {
            context.Response.StatusCode = 429;
            context.Response.Headers["Retry-After"] = "1";
            await context.Response.WriteAsync("Too many requests");
            return;
        }

        await _next(context);
    }
}
