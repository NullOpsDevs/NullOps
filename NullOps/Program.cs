using NullOps.Extensions;
using NullOps.Middlewares;
using NullOps.Setup;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.SetupLogging();

var startupLogger = Log.Logger.ForSourceContext("Startup");
startupLogger.Information("Starting NullOps");

builder.SetupWeb(startupLogger);
builder.SetupDatabase();
builder.SetupServices();

var app = builder.Build();

startupLogger.Information("Preparing database");

await app.MigrateDatabaseAsync();
await app.SeedDatabaseAsync();

startupLogger.Information("Initialized successfully, API will now serve requests");

app.UseMiddleware<ExceptionHandlerMiddleware>();
app.MapControllers();

if (app.Environment.IsDevelopment())
{
    startupLogger.Fatal("WARNING! You are running in development mode!");
    startupLogger.Fatal("WARNING! Development mode will allow ANYONE to reset the database, change settings, and more!");
    
    app.UseSwagger();
}

app.Run();
