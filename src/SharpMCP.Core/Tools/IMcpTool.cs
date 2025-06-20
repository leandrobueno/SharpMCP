using SharpMCP.Core.Protocol;
using System.Text.Json;

namespace SharpMCP.Core.Tools;

/// <summary>
/// Defines a tool that can be executed by an MCP server.
/// </summary>
public interface IMcpTool
{
    /// <summary>
    /// Gets the unique name of the tool.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Gets the description of what the tool does.
    /// </summary>
    string? Description { get; }

    /// <summary>
    /// Gets the JSON schema for the tool's input parameters.
    /// </summary>
    JsonElement GetInputSchema();

    /// <summary>
    /// Executes the tool with the provided arguments.
    /// </summary>
    /// <param name="arguments">The tool arguments as a JSON element.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>The tool response.</returns>
    Task<ToolResponse> ExecuteAsync(JsonElement? arguments, CancellationToken cancellationToken = default);
}

/// <summary>
/// Context information provided to a tool during execution.
/// </summary>
public class ToolExecutionContext
{
    /// <summary>
    /// Gets or sets the server instance.
    /// </summary>
    public object? Server { get; set; }

    /// <summary>
    /// Gets or sets custom context data.
    /// </summary>
    public Dictionary<string, object?> Items { get; set; } = [];
}

/// <summary>
/// Attribute to mark a class as an MCP tool.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class McpToolAttribute : Attribute
{
    /// <summary>
    /// Gets the tool name.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Gets or sets the tool description.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="McpToolAttribute"/> class.
    /// </summary>
    /// <param name="name">The tool name.</param>
    public McpToolAttribute(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        Name = name;
    }
}

/// <summary>
/// Exception thrown when a tool execution fails.
/// </summary>
public class McpToolException : Exception
{
    /// <summary>
    /// Gets or sets the error code.
    /// </summary>
    public int ErrorCode { get; set; } = JsonRpcErrorCodes.InternalError;

    /// <summary>
    /// Gets or sets whether the error is retryable.
    /// </summary>
    public bool IsRetryable { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="McpToolException"/> class.
    /// </summary>
    public McpToolException() { }

    /// <summary>
    /// Initializes a new instance of the <see cref="McpToolException"/> class with a message.
    /// </summary>
    public McpToolException(string message) : base(message) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="McpToolException"/> class with a message and inner exception.
    /// </summary>
    public McpToolException(string message, Exception innerException)
        : base(message, innerException) { }
}
