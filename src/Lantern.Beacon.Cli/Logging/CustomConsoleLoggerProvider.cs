using Microsoft.Extensions.Logging;

namespace Lantern.Beacon.Cli.Logging;

public class CustomConsoleLoggerProvider(
    Func<CustomConsoleLogger.CustomConsoleLoggerConfiguration, bool> filter,
    CustomConsoleLogger.CustomConsoleLoggerConfiguration config)
    : ILoggerProvider
{
    private readonly Func<CustomConsoleLogger.CustomConsoleLoggerConfiguration, bool> _filter = filter ?? throw new ArgumentNullException(nameof(filter));
    private readonly CustomConsoleLogger.CustomConsoleLoggerConfiguration _config = config ?? throw new ArgumentNullException(nameof(config));

    public ILogger CreateLogger(string categoryName)
    {
        return new CustomConsoleLogger(categoryName, _filter, _config);
    }

    public void Dispose()
    {
        
    }
}