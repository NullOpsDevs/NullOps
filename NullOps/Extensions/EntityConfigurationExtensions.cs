using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NullOps.DAL.Models;

namespace NullOps.Extensions;

public static class EntityConfigurationExtensions
{
    public static void SetupBase<TEntity>(this EntityTypeBuilder<TEntity> builder) 
        where TEntity : class, IEntity
    {
        builder.HasKey(x => x.Id);
        
        builder.Property(x => x.Id).IsRequired().ValueGeneratedOnAdd();
        builder.Property(x => x.CreatedAt).IsRequired().ValueGeneratedOnAdd();
        builder.Property(x => x.UpdatedAt).IsRequired().;
    }
}