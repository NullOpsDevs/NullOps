using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.ValueGeneration;

namespace NullOps.DAL.Configurations;

public class CreatedAtGenerator : ValueGenerator<DateTime>
{
    /// <inheritdoc />
    public override DateTime Next(EntityEntry entry) => DateTime.UtcNow;

    /// <inheritdoc />
    public override bool GeneratesTemporaryValues => false;
}