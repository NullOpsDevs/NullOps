using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NullOps.DAL.Models;
using NullOps.Extensions;

namespace NullOps.DAL.Configurations;

public class SettingConfiguration : IEntityTypeConfiguration<Setting>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<Setting> builder)
    {
        builder.ToTable("settings");
        
        builder.SetupBase();
        
        builder.Property(x => x.Key).IsRequired().HasMaxLength(Limits.Settings.MaxKeyLength);
        builder.Property(x => x.Value).IsRequired().HasMaxLength(Limits.Settings.MaxValueLength);
        
        builder.HasIndex(x => x.Key);
    }
}