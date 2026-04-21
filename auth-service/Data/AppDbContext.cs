using auth_service.Configurations.Auth;
using auth_service.Modules.Auth.Models;
using Microsoft.EntityFrameworkCore;

namespace auth_service.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options) { }

    public required DbSet<User> Users { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfiguration(new UserConfiguration());
    }
}
