using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using NullOps.DAL;
using NullOps.DAL.Enums;
using NullOps.Extensions;
using NullOps.Services.Seeding;

namespace NullOps.Setup;

public static class DatabaseSetup
{
    public static void SetupDatabase(this WebApplicationBuilder builder)
    {
        builder.Services.AddPooledDbContextFactory<DatabaseContext>(options =>
        {
            options.UseNpgsql(EnvSettings.Database.ConnectionString, DatabaseContext.MapEnums);
            options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
        });
    }

    public static async Task MigrateDatabaseAsync(this WebApplication app)
    {
        await using var context = await app.Services.CreateDatabaseContext();
        await context.Database.MigrateAsync();
    }

    public static async Task SeedDatabaseAsync(this WebApplication app)
    {
        await app.Services.GetRequiredService<RootSeederService>().SeedAsync();
    }
}

public class MigrationTimeDbContextSetup : IDesignTimeDbContextFactory<DatabaseContext>
{
    private class NewcomerContributorException : Exception;
    
    /// <inheritdoc />
    public DatabaseContext CreateDbContext(string[] args)
    {
        var localConnectionString = EnvSettings.Database.DevelopmentConnectionString;

        if (string.IsNullOrEmpty(localConnectionString))
        {
            Console.WriteLine("==============================================");
            Console.WriteLine("================ NULLOPS =====================");
            Console.WriteLine("==============================================");
            Console.WriteLine();
            Console.WriteLine($"Please set the '{localConnectionString.Key}' environment variable.");
            Console.WriteLine("EFCore and PgSQL together require an alive PgSQL database to create migrations.");
            Console.WriteLine();
            Console.WriteLine();

            throw new NewcomerContributorException(); // ;)
        }
        
        var optionsBuilder = new DbContextOptionsBuilder<DatabaseContext>();
        optionsBuilder.UseNpgsql(localConnectionString, DatabaseContext.MapEnums);
        
        return new DatabaseContext(optionsBuilder.Options);
    }
}