using SharpMCP.Core.Protocol;
using SharpMCP.Core.Tools;
using SharpMCP.Core.Transport;

namespace SharpMCP.Core.Server;

/// <summary>
/// Defines the core functionality of an MCP server.
/// </summary>
public interface IMcpServer
{
    /// <summary>
    /// Gets the server name.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Gets the server version.
    /// </summary>
    string Version { get; }

    /// <summary>
    /// Gets the server capabilities.
    /// </summary>
    ServerCapabilities Capabilities { get; }

    /// <summary>
    /// Gets the registered tools.
    /// </summary>
    IReadOnlyDictionary<string, IMcpTool> Tools { get; }

    /// <summary>
    /// Runs the server using the specified transport.
    /// </summary>
    /// <param name="transport">The transport to use for communication.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    Task RunAsync(IMcpTransport transport, CancellationToken cancellationToken = default);

    /// <summary>
    /// Registers a tool with the server.
    /// </summary>
    /// <param name="tool">The tool to register.</param>
    void RegisterTool(IMcpTool tool);

    /// <summary>
    /// Unregisters a tool from the server.
    /// </summary>
    /// <param name="toolName">The name of the tool to unregister.</param>
    /// <returns>True if the tool was unregistered, false if it wasn't found.</returns>
    bool UnregisterTool(string toolName);
}

/// <summary>
/// Configuration options for an MCP server.
/// </summary>
public class McpServerOptions
{
    /// <summary>
    /// Gets or sets the server name.
    /// </summary>
    public string Name { get; set; } = "SharpMCP Server";

    /// <summary>
    /// Gets or sets the server version.
    /// </summary>
    public string Version { get; set; } = "1.0.0";

    /// <summary>
    /// Gets or sets whether tools are enabled.
    /// </summary>
    public bool EnableTools { get; set; } = true;

    /// <summary>
    /// Gets or sets whether resources are enabled.
    /// </summary>
    public bool EnableResources { get; set; } = false;

    /// <summary>
    /// Gets or sets whether prompts are enabled.
    /// </summary>
    public bool EnablePrompts { get; set; } = false;

    /// <summary>
    /// Gets or sets the maximum concurrent tool executions.
    /// </summary>
    public int MaxConcurrentToolExecutions { get; set; } = 10;

    /// <summary>
    /// Gets or sets the tool execution timeout in seconds.
    /// </summary>
    public int ToolExecutionTimeoutSeconds { get; set; } = 300;
}

/// <summary>
/// Event arguments for server lifecycle events.
/// </summary>
public class McpServerEventArgs : EventArgs
{
    /// <summary>
    /// Gets the server instance.
    /// </summary>
    public IMcpServer Server { get; }

    /// <summary>
    /// Gets the event timestamp.
    /// </summary>
    public DateTimeOffset Timestamp { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="McpServerEventArgs"/> class.
    /// </summary>
    /// <param name="server">The server instance.</param>
    public McpServerEventArgs(IMcpServer server)
    {
        Server = server;
        Timestamp = DateTimeOffset.UtcNow;
    }
}

/// <summary>
/// Event arguments for tool execution events.
/// </summary>
public class ToolExecutionEventArgs : McpServerEventArgs
{
    /// <summary>
    /// Gets the tool name.
    /// </summary>
    public string ToolName { get; }

    /// <summary>
    /// Gets the execution duration.
    /// </summary>
    public TimeSpan? Duration { get; }

    /// <summary>
    /// Gets whether the execution was successful.
    /// </summary>
    public bool Success { get; }

    /// <summary>
    /// Gets the error message if the execution failed.
    /// </summary>
    public string? ErrorMessage { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ToolExecutionEventArgs"/> class.
    /// </summary>
    public ToolExecutionEventArgs(
        IMcpServer server,
        string toolName,
        bool success,
        TimeSpan? duration = null,
        string? errorMessage = null) : base(server)
    {
        ToolName = toolName;
        Success = success;
        Duration = duration;
        ErrorMessage = errorMessage;
    }
}
