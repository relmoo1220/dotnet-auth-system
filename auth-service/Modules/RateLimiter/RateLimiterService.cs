using StackExchange.Redis;

namespace auth_service.Modules.RateLimiter;

public class RateLimiterService
{
    private readonly IDatabase _db;
    private readonly string _lua;

    public RateLimiterService(IConnectionMultiplexer redis)
    {
        _db = redis.GetDatabase();

        _lua = File.ReadAllText(
            Path.Combine(AppContext.BaseDirectory, "Modules/RateLimiter/Scripts/token_bucket.lua")
        );
    }

    public async Task<(bool allowed, double tokensLeft)> AllowRequestAsync(
        string key,
        int capacity,
        int refillRate,
        int refillIntervalSeconds
    )
    {
        long now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

        RedisResult raw = await _db.ScriptEvaluateAsync(
            _lua,
            new RedisKey[] { key },
            new RedisValue[] { capacity, refillRate, refillIntervalSeconds, now }
        );

        if (raw.IsNull)
            throw new InvalidOperationException("Redis returned null");

        var allowed = Convert.ToInt32(raw[0]) == 1;
        var tokensLeft = Convert.ToDouble(raw[1]);

        return (allowed, tokensLeft);
    }
}
