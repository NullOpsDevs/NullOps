using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NullOps.DAL.Models;
using NullOps.Extensions;

namespace NullOps.DAL.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("users");
        
        builder.SetupBase();
        
        builder.Property(x => x.Username).IsRequired().HasMaxLength(Limits.Users.MaxUsernameLength);
        builder.Property(x => x.Password).IsRequired().HasMaxLength(Limits.Users.MaxPasswordLength);
        builder.Property(x => x.Role).IsRequired();
        builder.Property(x => x.IsBlocked).IsRequired().HasDefaultValue(false);
        
        builder.HasIndex(user => user.Username).IsUnique();
    }
}
