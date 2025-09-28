using Serilog.Core.Enrichers;
using ILogger = Serilog.ILogger;

namespace NullOps.Extensions;

public static class LoggerExtensions
{
    public static ILogger ForSourceContext(this ILogger logger, string context) => logger.ForContext(new PropertyEnricher("SourceContext", context));
}