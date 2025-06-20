using Microsoft.Extensions.Logging;
using SharpMCP.Server.Logging;

namespace SharpMCP.Server;

/// <summary>
/// Extension methods for configuring MCP-safe logging.
/// </summary>
public static class McpLoggingExtensions
{
    /// <summary>
    /// Adds MCP-safe console logging that writes to stderr instead of stdout.
    /// This prevents interference with MCP JSON-RPC communication on stdout.
    /// </summary>
    /// <param name="builder">The logging builder.</param>
    /// <param name="minimumLevel">The minimum log level. Defaults to Information.</param>
    /// <returns>The logging builder for chaining.</returns>
    public static ILoggingBuilder AddMcpConsole(this ILoggingBuilder builder, LogLevel minimumLevel = LogLevel.Information)
    {
        return builder.AddProvider(new McpConsoleLoggerProvider(minimumLevel));
    }

    /// <summary>
    /// Adds MCP-safe console logging with the specified configuration.
    /// This prevents interference with MCP JSON-RPC communication on stdout.
    /// </summary>
    /// <param name="builder">The logging builder.</param>
    /// <param name="configure">Configuration action for the logger.</param>
    /// <returns>The logging builder for chaining.</returns>
    public static ILoggingBuilder AddMcpConsole(this ILoggingBuilder builder, Action<McpConsoleLoggerConfiguration> configure)
    {
        var config = new McpConsoleLoggerConfiguration();
        configure(config);
        return builder.AddProvider(new McpConsoleLoggerProvider(config.MinimumLevel));
    }
}

/// <summary>
/// Configuration options for MCP console logger.
/// </summary>
public class McpConsoleLoggerConfiguration
{
    /// <summary>
    /// Gets or sets the minimum log level.
    /// </summary>
    public LogLevel MinimumLevel { get; set; } = LogLevel.Information;
}
