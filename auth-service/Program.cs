using System.Text;
using System.Text.Json;
using auth_service.Data;
using auth_service.Infrastructure.StartupChecks;
using auth_service.Modules.Auth.Models;
using auth_service.Modules.Auth.Services;
using auth_service.Modules.RateLimiter;
using auth_service.Options;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using StackExchange.Redis;

Log.Logger = new LoggerConfiguration().Enrich.FromLogContext().WriteTo.Console().CreateLogger();

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog();

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddControllers();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();
builder.Services.AddScoped<IJwtService, JwtService>();
builder.Services.AddScoped<IRefreshTokenService, RefreshTokenService>();

builder
    .Services.AddOptions<DatabaseOptions>()
    .Bind(builder.Configuration.GetSection("Database"))
    .Validate(o => !string.IsNullOrWhiteSpace(o.Postgres))
    .ValidateOnStart();

builder
    .Services.AddOptions<RedisOptions>()
    .Bind(builder.Configuration.GetSection("Redis"))
    .Validate(o => !string.IsNullOrWhiteSpace(o.ConnectionString))
    .ValidateOnStart();

builder
    .Services.AddOptions<JwtOptions>()
    .Bind(builder.Configuration.GetSection("Jwt"))
    .Validate(o => !string.IsNullOrWhiteSpace(o.Key), "JWT Key is required")
    .Validate(o => o.ExpiryMinutes > 0, "Expiry must be > 0")
    .ValidateOnStart();

builder.Services.AddDbContext<AppDbContext>(
    (sp, options) =>
    {
        var db = sp.GetRequiredService<IOptions<DatabaseOptions>>().Value;

        options.UseNpgsql(db.Postgres);
    }
);

builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
{
    var redis = sp.GetRequiredService<IOptions<RedisOptions>>().Value;

    return ConnectionMultiplexer.Connect(redis.ConnectionString);
});

builder.Services.AddSingleton<RateLimiterService>();

builder
    .Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        var jwt = builder.Configuration.GetSection("Jwt").Get<JwtOptions>()!;

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,

            ValidIssuer = jwt.Issuer,
            ValidAudience = jwt.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.Key)),
        };
    });

builder.Services.AddAuthorization();

builder.Services.AddCors(options =>
{
    options.AddPolicy(
        "AllowAngularApp",
        policy =>
        {
            policy
                .WithOrigins("http://localhost:4200")
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials();
        }
    );
});

builder
    .Services.AddHealthChecks()
    .AddDbContextCheck<AppDbContext>()
    .AddRedis(builder.Configuration["Redis:ConnectionString"]!);

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    AppStartupChecks.Run(scope.ServiceProvider);
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseSerilogRequestLogging();
app.UseHttpsRedirection();
app.UseCors("AllowAngularApp");
app.UseAuthentication();
app.UseAuthorization();
app.UseMiddleware<RateLimiterMiddleware>();
app.MapHealthChecks(
    "/healthz",
    new HealthCheckOptions
    {
        ResponseWriter = async (context, report) =>
        {
            context.Response.ContentType = "application/json";

            var result = new
            {
                status = report.Status.ToString(),
                totalDuration = report.TotalDuration,
                checks = report.Entries.Select(entry => new
                {
                    name = entry.Key,
                    status = entry.Value.Status.ToString(),
                    duration = entry.Value.Duration,
                    description = entry.Value.Description,
                    exception = entry.Value.Exception?.Message,
                }),
            };

            await context.Response.WriteAsync(
                JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true })
            );
        },
    }
);
app.MapControllers();

app.Run();
