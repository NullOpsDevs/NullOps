using Microsoft.EntityFrameworkCore;
using NullOps.DAL;
using NullOps.DAL.Enums;
using NullOps.DAL.Models;
using NullOps.Services.Users.Hashers;

namespace NullOps.Services.Seeding.Seeders;

public class SuperAdminSeederService(IUserPasswordHasher userPasswordHasher, ILogger<SuperAdminSeederService> logger) : ISeeder
{
    public const string SuperAdminCredentials = "admin";
    
    /// <inheritdoc />
    public async Task<bool> IsSeedingRequired(DatabaseContext context)
    {
        return !await context.Users.AnyAsync(x => x.Username == SuperAdminCredentials);
    }

    /// <inheritdoc />
    public SeedPriority Priority => SeedPriority.High;

    /// <inheritdoc />
    public async Task SeedAsync(DatabaseContext context)
    {
        var superAdminUser = new User
        {
            Username = SuperAdminCredentials,
            Password = string.Empty,
            Role = UserRole.SuperAdministrator
        };
        
        await context.Users.AddAsync(superAdminUser);
        await context.SaveChangesAsync();

        await context.Users.Where(x => x.Id == superAdminUser.Id)
            .ExecuteUpdateAsync(x =>
                x.SetProperty(p => p.Password, userPasswordHasher.Hash(superAdminUser.Id, SuperAdminCredentials)));
        
        logger.LogInformation("Super admin user '{Username}' with password '{Password}' created, please change password as soon as possible", SuperAdminCredentials, SuperAdminCredentials);
    }
}