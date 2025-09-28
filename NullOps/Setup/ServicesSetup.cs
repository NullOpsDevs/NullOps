using NullOps.Middlewares;
using NullOps.Services.Seeding;
using NullOps.Services.Seeding.Seeders;
using NullOps.Services.Users;
using NullOps.Services.Users.Hashers;

namespace NullOps.Setup;

public static class ServicesSetup
{
    public static void SetupServices(this WebApplicationBuilder builder)
    {
        // Seeding
        builder.Services.AddSingleton<RootSeederService>();
        builder.Services.AddSingleton<ISeeder, SuperAdminSeederService>();
        
        // Password hashing
        builder.Services.AddSingleton<IUserPasswordHasher, BCryptPasswordHasher>();
        
        // Middlewares
        builder.Services.AddSingleton<ExceptionHandlerMiddleware>();
        
        // Domain services
        builder.Services.AddSingleton<UserLoginService>();
    }
}