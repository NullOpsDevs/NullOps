using Microsoft.EntityFrameworkCore;
using NullOps.DAL;
using NullOps.DAL.Enums;
using NullOps.DAL.Models;
using NullOps.Services.Users.Hashers;

namespace NullOps.Services.Seeding.Seeders;

public class SuperAdminSeederService(IUserPasswordHasher userPasswordHasher, ILogger<SuperAdminSeederService> logger) : ISeeder
{
    private const string SuperAdmin = "admin";
    
    /// <inheritdoc />
    public async Task<bool> IsSeedingRequired(DatabaseContext context)
    {
        return !await context.Users.AnyAsync(x => x.Username == SuperAdmin);
    }

    /// <inheritdoc />
    public SeedPriority Priority => SeedPriority.High;

    /// <inheritdoc />
    public async Task SeedAsync(DatabaseContext context)
    {
        var superAdminUser = new User
        {
            Username = SuperAdmin,
            Password = string.Empty,
            Role = UserRole.SuperAdministrator
        };
        
        await context.Users.AddAsync(superAdminUser);
        await context.SaveChangesAsync();

        await context.Users.Where(x => x.Id == superAdminUser.Id)
            .ExecuteUpdateAsync(x =>
                x.SetProperty(p => p.Password, userPasswordHasher.Hash(superAdminUser.Id, SuperAdmin)));
        
        logger.LogInformation("Super admin user '{Username}' with password '{Password}' created, please change password as soon as possible", SuperAdmin, SuperAdmin);
    }
}