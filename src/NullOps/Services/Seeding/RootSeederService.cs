using Microsoft.EntityFrameworkCore;
using NullOps.DAL;

namespace NullOps.Services.Seeding;

public class RootSeederService(IEnumerable<ISeeder> seeders, ILogger<RootSeederService> logger, IDbContextFactory<DatabaseContext> dbContextFactory)
{
    public async Task SeedAsync()
    {
        await using var context = await dbContextFactory.CreateDbContextAsync();
        
        logger.LogInformation("Database seeding started");
        
        var activeSeeders = new List<ISeeder>();

        foreach (var seeder in seeders)
        {
            var seedingRequired = await seeder.IsSeedingRequired(context);
            
            if(!seedingRequired)
                continue;
            
            activeSeeders.Add(seeder);
        }

        foreach (var seeder in activeSeeders.OrderBy(x => x.Priority))
        {
            logger.LogInformation("Seeding '{SeederType}'...", seeder.GetType().Name);
            await seeder.SeedAsync(context);
        }
    }
}