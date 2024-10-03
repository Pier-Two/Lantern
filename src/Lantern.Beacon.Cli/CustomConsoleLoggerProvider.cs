using Microsoft.Extensions.Logging;

namespace Lantern.Beacon.Console;

public class CustomConsoleLoggerProvider : ILoggerProvider
{
    private readonly Func<CustomConsoleLogger.CustomConsoleLoggerConfiguration, bool> _filter;
    private readonly CustomConsoleLogger.CustomConsoleLoggerConfiguration _config;

    public CustomConsoleLoggerProvider(
        Func<CustomConsoleLogger.CustomConsoleLoggerConfiguration, bool> filter,
        CustomConsoleLogger.CustomConsoleLoggerConfiguration config)
    {
        _filter = filter ?? throw new ArgumentNullException(nameof(filter));
        _config = config ?? throw new ArgumentNullException(nameof(config));
    }

    public ILogger CreateLogger(string categoryName)
    {
        return new CustomConsoleLogger(categoryName, _filter, _config);
    }

    public void Dispose()
    {
        // Cleanup, if necessary. For now, no unmanaged resources are being used.
    }
}