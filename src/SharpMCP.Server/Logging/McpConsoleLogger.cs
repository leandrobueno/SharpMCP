using Microsoft.Extensions.Logging;

namespace SharpMCP.Server.Logging;

/// <summary>
/// Console logger provider that writes to stderr instead of stdout to avoid
/// interfering with MCP JSON-RPC communication on stdout.
/// </summary>
public sealed class McpConsoleLoggerProvider : ILoggerProvider
{
    private readonly LogLevel _minimumLevel;

    /// <summary>
    /// Initializes a new instance of the <see cref="McpConsoleLoggerProvider"/> class.
    /// </summary>
    /// <param name="minimumLevel">The minimum log level to output.</param>
    public McpConsoleLoggerProvider(LogLevel minimumLevel = LogLevel.Information)
    {
        _minimumLevel = minimumLevel;
    }

    /// <inheritdoc />
    public ILogger CreateLogger(string categoryName) => new McpConsoleLogger(categoryName, _minimumLevel);

    /// <inheritdoc />
    public void Dispose()
    {
        // Nothing to dispose
    }
}

/// <summary>
/// Console logger that writes to stderr instead of stdout to avoid interfering
/// with MCP JSON-RPC communication.
/// </summary>
internal sealed class McpConsoleLogger : ILogger
{
    private readonly string _categoryName;
    private readonly LogLevel _minimumLevel;

    /// <summary>
    /// Initializes a new instance of the <see cref="McpConsoleLogger"/> class.
    /// </summary>
    /// <param name="categoryName">The category name for this logger.</param>
    /// <param name="minimumLevel">The minimum log level to output.</param>
    public McpConsoleLogger(string categoryName, LogLevel minimumLevel)
    {
        _categoryName = categoryName;
        _minimumLevel = minimumLevel;
    }

    /// <inheritdoc />
    public IDisposable? BeginScope<TState>(TState state) where TState : notnull => NullScope.Instance;

    /// <inheritdoc />
    public bool IsEnabled(LogLevel logLevel) => logLevel >= _minimumLevel;

    /// <inheritdoc />
    public void Log<TState>(
        LogLevel logLevel,
        EventId eventId,
        TState state,
        Exception? exception,
        Func<TState, Exception?, string> formatter)
    {
        if (!IsEnabled(logLevel))
        {
            return;
        }

        var message = formatter(state, exception);
        var timestamp = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff");
        var levelStr = GetLevelString(logLevel);
        var categoryShort = GetShortCategoryName(_categoryName);

        // CRITICAL: Always write to stderr, never stdout (which is used for JSON-RPC)
        Console.Error.WriteLine($"[{timestamp}] [{levelStr}] {categoryShort}: {message}");

        if (exception != null)
        {
            Console.Error.WriteLine($"[{timestamp}] [{levelStr}] {categoryShort}: Exception: {exception}");
        }
    }

    private static string GetLevelString(LogLevel logLevel) => logLevel switch
    {
        LogLevel.Trace => "TRACE",
        LogLevel.Debug => "DEBUG",
        LogLevel.Information => "INFO ",
        LogLevel.Warning => "WARN ",
        LogLevel.Error => "ERROR",
        LogLevel.Critical => "CRIT ",
        _ => "UNKN "
    };

    private static string GetShortCategoryName(string categoryName)
    {
        // Shorten long category names for cleaner output
        var parts = categoryName.Split('.');
        return parts.Length > 2 ? $"{parts[0]}...{parts[^1]}" : categoryName;
    }

    /// <summary>
    /// Null scope implementation for logger scopes.
    /// </summary>
    private sealed class NullScope : IDisposable
    {
        /// <summary>
        /// Gets the singleton instance.
        /// </summary>
        public static NullScope Instance { get; } = new();

        /// <inheritdoc />
        public void Dispose()
        {
            // Nothing to dispose
        }
    }
}
