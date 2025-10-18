using Microsoft.EntityFrameworkCore;
using NullOps.DAL;
using NullOps.DAL.Models;

namespace NullOps.Extensions;

public static class DbContextExtensions
{
    public static async Task<string?> GetConfigurationAsync(this DatabaseContext context, string key)
    {
        var setting = await context.Settings.FirstOrDefaultAsync(x => x.Key == key);
        return setting?.Value;
    }
    
    public static async Task SetConfigurationAsync(this DatabaseContext context, string key, string value)
    {
        var setting = await context.Settings.FirstOrDefaultAsync(x => x.Key == key);
        
        if(setting == null)
        {
            setting = new Setting
            {
                Key = key,
                Value = value
            };
            
            context.Settings.Add(setting);
            await context.SaveChangesAsync();
            return;
        }

        await context.Settings.ExecuteUpdateAsync(x =>
            x.SetProperty(p => p.Value, value)
             .SetProperty(p => p.UpdatedAt, DateTime.UtcNow));
    }

    public static async Task<T?> GetConfigurationAsync<T>(this DatabaseContext context, string key, T? defaultValue = default)
    {
        var value = await GetConfigurationAsync(context, key);

        if (string.IsNullOrWhiteSpace(value))
            return defaultValue;
        
        return (T?) Convert.ChangeType(value, typeof(T));
    }

    public static async Task SetConfigurationAsync<T>(this DatabaseContext context, string key, T value)
    {
        await SetConfigurationAsync(context, key, ((string?) Convert.ChangeType(value, typeof(string))) ?? string.Empty);
    }
}