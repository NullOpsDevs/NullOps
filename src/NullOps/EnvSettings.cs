using System.Runtime.CompilerServices;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using NullOps.Extensions;
using Serilog;
using Serilog.Events;
using ILogger = Serilog.ILogger;

namespace NullOps;

public static class EnvSettings
{
    private static readonly ILogger Logger = Log.Logger.ForSourceContext("Configuration");
    
    public class EnvironmentVariable<T>(string key, T defaultValue, Func<T, bool>? validator = null)
    {
        private readonly Lazy<T> valueFactory = new(() => GetValue(key, defaultValue, validator));
        
        public string Key { get; } = key;
        public T Value => valueFactory.Value;
        
        private static T GetValue(string key, T defaultValue, Func<T, bool>? validator)
        {
            var value = Environment.GetEnvironmentVariable(key);

            if (string.IsNullOrEmpty(value))
            {
                Logger.Warning("Environment variable '{Key}' is not set. Using default value: '{DefaultValue}'.",
                    key, defaultValue);
                
                return defaultValue;
            }

            try
            {
                var converted = (T) Convert.ChangeType(value, typeof(T));

                if (validator != null && !validator(converted))
                {
                    Logger.Warning("Environment variable '{Key}' has invalid value '{Value}'. Using default value: '{DefaultValue}'.",
                        key, value, defaultValue);
                    
                    return defaultValue;
                }

                return converted;
            } catch (Exception)
            {
                Logger.Warning("Unable to parse environment variable '{Key}' with value '{Value}'. Using default value: '{DefaultValue}'.",
                    key, value, defaultValue);
                
                return defaultValue;
            }
        }
        
        public static implicit operator T(EnvironmentVariable<T> variable) => variable.Value;
    }
    
    public static class Logging
    {
        public static readonly EnvironmentVariable<bool> ConsoleJson = new("NOPS_LOGGING_CONSOLE_JSON", false);
        public static readonly EnvironmentVariable<int> ConsoleLogLevel = new("NOPS_LOGGING_CONSOLE_LOG_LEVEL", 2, EnumValidator<LogEventLevel>);
        
        public static readonly EnvironmentVariable<bool> File = new("NOPS_LOGGING_FILE", false);
        public static readonly EnvironmentVariable<string> FolderPath = new("NOPS_LOGGING_FILE_FOLDER_PATH", "logs");
        public static readonly EnvironmentVariable<bool> FileJson = new("NOPS_LOGGING_FILE_JSON", false);
        public static readonly EnvironmentVariable<int> FileLogLevel = new("NOPS_LOGGING_FILE_LOG_LEVEL", 2, EnumValidator<LogEventLevel>);
    }

    public static class Hosting
    {
        public static readonly EnvironmentVariable<int> Port = new("NOPS_API_PORT", 7000, PortValidator);
    }

    public static class Database
    {
        public static readonly EnvironmentVariable<string> ConnectionString = new("NOPS_DATABASE_CONNECTION_STRING", "Host=localhost");
        public static readonly EnvironmentVariable<string> DevelopmentConnectionString = new("NOPS_DEV_DATABASE_CONNECTION_STRING", string.Empty);
    }

    public static class Jwt
    {
        public static readonly EnvironmentVariable<string> Secret = new("NOPS_JWT_SECRET", Guid.NewGuid().ToString("N"), JwtSecretValidator);
        
        private static readonly Lazy<SymmetricSecurityKey> LazySigningKey = new(() => new SymmetricSecurityKey(Encoding.ASCII.GetBytes(Secret)));
        public static SymmetricSecurityKey SigningKey => LazySigningKey.Value;
    }
    
    private static bool EnumValidator<TEnum>(int value) where TEnum : struct, Enum
    {
        var valid = Enum.IsDefined(Unsafe.As<int, TEnum>(ref value));

        if (valid)
            return true;
        
        Logger.Warning("Failed to validate environment variable value: '{Value}'.", value);
        Logger.Warning("Expected values are:");

        foreach (var possibleEnumValue in Enum.GetValues<TEnum>())
        {
            Logger.Warning("{Value:G} - {Name:N}", possibleEnumValue, possibleEnumValue);
        }
        
        return false;
    }

    private static bool PortValidator(int port)
    {
        if (port is < 1025 or > 65535)
        {
            Logger.Warning("Port must be between 1025 and 65535.");
            return false;
        }

        return true;
    }

    private static bool JwtSecretValidator(string secret)
    {
        if (secret.Length != 32)
        {
            Logger.Warning("JWT secret must be 32 characters long.");
            return false;
        }

        return true;
    }
}