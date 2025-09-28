using NullOps.DAL;

namespace NullOps.Services.Seeding;

public interface ISeeder
{
    Task<bool> IsSeedingRequired(DatabaseContext context);
    
    SeedPriority Priority { get; }
    
    Task SeedAsync(DatabaseContext context);
}
