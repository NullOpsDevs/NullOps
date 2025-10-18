using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NullOps.DAL.Models;
using NullOps.Extensions;

namespace NullOps.DAL.Configurations;

public class AgentConfiguration : IEntityTypeConfiguration<Agent>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<Agent> builder)
    {
        builder.ToTable("agents");
        
        builder.SetupBase();

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(Limits.Agents.MaxNameLength);

        builder.Property(x => x.Address)
            .IsRequired()
            .HasMaxLength(Limits.Agents.MaxAddressLength);

        builder.Property(x => x.Port)
            .IsRequired();

        builder.Property(x => x.Token)
            .IsRequired()
            .HasMaxLength(Limits.Agents.MaxTokenLength);
        
        builder.HasIndex(x => x.Name).IsUnique();
    }
}