using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NullOps.DAL.Models;

namespace NullOps.DAL.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("users");
        
        builder.HasKey(x => x.Id);
        
        builder.Property(x => x.Id).IsRequired().ValueGeneratedOnAdd();
        builder.Property(x => x.Username).IsRequired().HasMaxLength(128);
        builder.Property(x => x.Password).IsRequired().HasMaxLength(256);
        builder.Property(x => x.Role).IsRequired();
    }
}