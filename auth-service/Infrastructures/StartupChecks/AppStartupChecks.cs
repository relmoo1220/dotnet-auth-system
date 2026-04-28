using auth_service.Data;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;

namespace auth_service.Infrastructure.StartupChecks;

public static class AppStartupChecks
{
    public static void Run(IServiceProvider sp)
    {
        CheckRedis(sp);
        CheckDatabase(sp);
    }

    private static void CheckRedis(IServiceProvider sp)
    {
        var redis = sp.GetRequiredService<IConnectionMultiplexer>();

        if (!redis.IsConnected)
            throw new Exception("Redis is not reachable at startup");

        redis.GetDatabase().Ping();
    }

    private static void CheckDatabase(IServiceProvider sp)
    {
        var db = sp.GetRequiredService<AppDbContext>();

        if (!db.Database.CanConnect())
            throw new Exception("Database is not reachable at startup");
        db.Database.Migrate();
    }
}
