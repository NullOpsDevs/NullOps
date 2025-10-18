using Serilog;
using Serilog.Events;
using Serilog.Formatting.Compact;

namespace NullOps.Setup;

public static class LoggingSetup
{
    private const string OutputTemplate = "[{Timestamp:HH:mm:ss}] [{Level:u3}] [{SourceContext}] {Message:lj}{NewLine}{Exception}";
    
    public static void SetupLogging(this WebApplicationBuilder builder)
    {
        // Setting up temporary global logger for EnvSettings
        Log.Logger = new LoggerConfiguration()
            .WriteTo.Console(outputTemplate: OutputTemplate)
            .CreateLogger();

        var loggerBuilder = new LoggerConfiguration()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Warning);

        if (!EnvSettings.Logging.ConsoleJson)
        {
            loggerBuilder.WriteTo.Console(
                outputTemplate:           OutputTemplate,
                restrictedToMinimumLevel: (LogEventLevel) (int) EnvSettings.Logging.ConsoleLogLevel);
        }
        else
        {
            loggerBuilder.WriteTo.Console(
                formatter:                new CompactJsonFormatter(),
                restrictedToMinimumLevel: (LogEventLevel) (int) EnvSettings.Logging.ConsoleLogLevel);
        }

        if (EnvSettings.Logging.File)
        {
            Directory.CreateDirectory(EnvSettings.Logging.FolderPath);

            if (!EnvSettings.Logging.FileJson)
            {
                loggerBuilder.WriteTo.File(
                    path:                     Path.Combine(EnvSettings.Logging.FolderPath, "nullops.log"),
                    outputTemplate:           OutputTemplate,
                    rollingInterval:          RollingInterval.Day,
                    rollOnFileSizeLimit:      true,
                    restrictedToMinimumLevel: (LogEventLevel) (int) EnvSettings.Logging.FileLogLevel);
            }
            else
            {
                loggerBuilder.WriteTo.File(
                    formatter:                new CompactJsonFormatter(),
                    path:                     Path.Combine(EnvSettings.Logging.FolderPath, "nullops.log"),
                    rollingInterval:          RollingInterval.Day,
                    rollOnFileSizeLimit:      true,
                    restrictedToMinimumLevel: (LogEventLevel) (int) EnvSettings.Logging.FileLogLevel);
            }
        }
        
        Log.Logger = loggerBuilder.CreateLogger();
        
        builder.Logging.ClearProviders();
        builder.Logging.AddSerilog(Log.Logger, true);
    }
}