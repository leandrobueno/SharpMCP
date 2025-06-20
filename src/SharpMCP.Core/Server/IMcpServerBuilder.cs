using Microsoft.Extensions.Logging;
using SharpMCP.Core.Tools;
using SharpMCP.Core.Transport;

namespace SharpMCP.Core.Server;

/// <summary>
/// Builder interface for constructing MCP servers.
/// </summary>
public interface IMcpServerBuilder
{
    /// <summary>
    /// Sets the server name.
    /// </summary>
    /// <param name="name">The server name.</param>
    /// <returns>The builder instance.</returns>
    IMcpServerBuilder WithName(string name);

    /// <summary>
    /// Sets the server version.
    /// </summary>
    /// <param name="version">The server version.</param>
    /// <returns>The builder instance.</returns>
    IMcpServerBuilder WithVersion(string version);

    /// <summary>
    /// Sets the server name, version, and optional description.
    /// </summary>
    /// <param name="name">The server name.</param>
    /// <param name="version">The server version.</param>
    /// <param name="description">Optional server description.</param>
    /// <returns>The builder instance.</returns>
    IMcpServerBuilder WithServerInfo(string name, string version, string? description = null);

    /// <summary>
    /// Adds a tool to the server.
    /// </summary>
    /// <param name="tool">The tool instance.</param>
    /// <returns>The builder instance.</returns>
    IMcpServerBuilder AddTool(IMcpTool tool);

    /// <summary>
    /// Adds a tool to the server by type.
    /// </summary>
    /// <typeparam name="TTool">The tool type.</typeparam>
    /// <returns>The builder instance.</returns>
    IMcpServerBuilder AddTool<TTool>() where TTool : IMcpTool, new();

    /// <summary>
    /// Adds multiple tools from an assembly.
    /// </summary>
    /// <param name="assembly">The assembly to scan for tools.</param>
    /// <returns>The builder instance.</returns>
    IMcpServerBuilder AddToolsFromAssembly(System.Reflection.Assembly assembly);

    /// <summary>
    /// Adds multiple tools from the assembly containing the specified type.
    /// </summary>
    /// <typeparam name="TAssemblyMarker">Type to use for assembly location.</typeparam>
    /// <returns>The builder instance.</returns>
    IMcpServerBuilder AddToolsFromAssembly<TAssemblyMarker>();

    /// <summary>
    /// Configures server options.
    /// </summary>
    /// <param name="configureOptions">Action to configure options.</param>
    /// <returns>The builder instance.</returns>
    IMcpServerBuilder ConfigureOptions(Action<McpServerOptions> configureOptions);

    /// <summary>
    /// Sets the logger factory.
    /// </summary>
    /// <param name="loggerFactory">The logger factory.</param>
    /// <returns>The builder instance.</returns>
    IMcpServerBuilder WithLoggerFactory(ILoggerFactory loggerFactory);

    /// <summary>
    /// Sets the logger factory. Alias for WithLoggerFactory.
    /// </summary>
    /// <param name="loggerFactory">The logger factory.</param>
    /// <returns>The builder instance.</returns>
    IMcpServerBuilder WithLogging(ILoggerFactory loggerFactory);

    /// <summary>
    /// Sets the transport to use.
    /// </summary>
    /// <param name="transport">The transport instance.</param>
    /// <returns>The builder instance.</returns>
    IMcpServerBuilder WithTransport(IMcpTransport transport);

    /// <summary>
    /// Uses standard I/O transport.
    /// </summary>
    /// <returns>The builder instance.</returns>
    IMcpServerBuilder UseStdio();

    /// <summary>
    /// Uses standard I/O transport. Alias for UseStdio.
    /// </summary>
    /// <returns>The builder instance.</returns>
    IMcpServerBuilder WithStdioTransport();

    /// <summary>
    /// Builds the MCP server.
    /// </summary>
    /// <returns>The configured server instance.</returns>
    IMcpServer Build();

    /// <summary>
    /// Builds and runs the MCP server.
    /// </summary>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>Task representing the server execution.</returns>
    Task BuildAndRunAsync(CancellationToken cancellationToken = default);
}

/// <summary>
/// Extension methods for MCP server builder.
/// </summary>
public static class McpServerBuilderExtensions
{
    /// <summary>
    /// Adds tools from the calling assembly.
    /// </summary>
    public static IMcpServerBuilder AddToolsFromCurrentAssembly(this IMcpServerBuilder builder)
    {
        return builder.AddToolsFromAssembly(System.Reflection.Assembly.GetCallingAssembly());
    }

    /// <summary>
    /// Enables tool capabilities.
    /// </summary>
    public static IMcpServerBuilder EnableTools(this IMcpServerBuilder builder, bool enable = true)
    {
        return builder.ConfigureOptions(options => options.EnableTools = enable);
    }

    /// <summary>
    /// Enables resource capabilities.
    /// </summary>
    public static IMcpServerBuilder EnableResources(this IMcpServerBuilder builder, bool enable = true)
    {
        return builder.ConfigureOptions(options => options.EnableResources = enable);
    }

    /// <summary>
    /// Enables prompt capabilities.
    /// </summary>
    public static IMcpServerBuilder EnablePrompts(this IMcpServerBuilder builder, bool enable = true)
    {
        return builder.ConfigureOptions(options => options.EnablePrompts = enable);
    }

    /// <summary>
    /// Sets the tool execution timeout.
    /// </summary>
    public static IMcpServerBuilder WithToolTimeout(this IMcpServerBuilder builder, TimeSpan timeout)
    {
        return builder.ConfigureOptions(options =>
            options.ToolExecutionTimeoutSeconds = (int)timeout.TotalSeconds);
    }
}
