using Microsoft.EntityFrameworkCore;
using NullOps.DAL;

namespace NullOps.Extensions;

public static class ServiceProviderExtensions
{
    public static async Task<DatabaseContext> CreateDatabaseContext(this IServiceProvider provider)
    {
        return await provider.GetRequiredService<IDbContextFactory<DatabaseContext>>().CreateDbContextAsync();
    }
}