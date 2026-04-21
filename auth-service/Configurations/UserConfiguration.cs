using auth_service.Modules.Auth.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace auth_service.Configurations.Auth;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.HasKey(u => u.Id);

        builder.Property(u => u.Username).IsRequired().HasMaxLength(50).HasColumnType("text");

        builder.HasIndex(u => u.Username).IsUnique();

        builder.Property(u => u.Password).IsRequired().HasColumnType("text");
    }
}
