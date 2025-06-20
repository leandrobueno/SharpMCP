using System.Text.Json;
using System.Text.Json.Serialization;

namespace SharpMCP.Core.Protocol;

/// <summary>
/// MCP protocol constants.
/// </summary>
public static class McpConstants
{
    /// <summary>
    /// The MCP protocol version.
    /// </summary>
    public const string ProtocolVersion = "2024-11-05";
}

/// <summary>
/// Server information returned during initialization.
/// </summary>
public class ServerInfo
{
    /// <summary>
    /// Gets or sets the protocol version supported by the server.
    /// </summary>
    [JsonPropertyName("protocolVersion")]
    public string ProtocolVersion { get; set; } = McpConstants.ProtocolVersion;

    /// <summary>
    /// Gets or sets the server capabilities.
    /// </summary>
    [JsonPropertyName("capabilities")]
    public ServerCapabilities Capabilities { get; set; } = new();

    /// <summary>
    /// Gets or sets the server metadata.
    /// </summary>
    [JsonPropertyName("serverInfo")]
    public ServerMetadata ServerMetadata { get; set; } = new() { Name = "UnnamedServer", Version = "0.0.0" };
}

/// <summary>
/// Server metadata information.
/// </summary>
public class ServerMetadata
{
    /// <summary>
    /// Gets or sets the server name.
    /// </summary>
    [JsonPropertyName("name")]
    public required string Name { get; set; }

    /// <summary>
    /// Gets or sets the server version.
    /// </summary>
    [JsonPropertyName("version")]
    public required string Version { get; set; }
}

/// <summary>
/// Capabilities supported by the server.
/// </summary>
public class ServerCapabilities
{
    /// <summary>
    /// Gets or sets the tools capability.
    /// </summary>
    [JsonPropertyName("tools")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public ToolsCapability? Tools { get; set; }

    /// <summary>
    /// Gets or sets the resources capability.
    /// </summary>
    [JsonPropertyName("resources")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public ResourcesCapability? Resources { get; set; }

    /// <summary>
    /// Gets or sets the prompts capability.
    /// </summary>
    [JsonPropertyName("prompts")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public PromptsCapability? Prompts { get; set; }
}

/// <summary>
/// Indicates that the server supports tools.
/// </summary>
public class ToolsCapability
{
}

/// <summary>
/// Indicates that the server supports resources.
/// </summary>
public class ResourcesCapability
{
    /// <summary>
    /// Gets or sets whether the server supports resource subscriptions.
    /// </summary>
    [JsonPropertyName("subscribe")]
    public bool Subscribe { get; set; }

    /// <summary>
    /// Gets or sets whether the server supports list changed notifications.
    /// </summary>
    [JsonPropertyName("listChanged")]
    public bool ListChanged { get; set; }
}

/// <summary>
/// Indicates that the server supports prompts.
/// </summary>
public class PromptsCapability
{
}

/// <summary>
/// Tool definition.
/// </summary>
public class Tool
{
    /// <summary>
    /// Gets or sets the tool name.
    /// </summary>
    [JsonPropertyName("name")]
    public required string Name { get; set; }

    /// <summary>
    /// Gets or sets the tool description.
    /// </summary>
    [JsonPropertyName("description")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets the JSON schema for the tool's input parameters.
    /// </summary>
    [JsonPropertyName("inputSchema")]
    public required JsonElement InputSchema { get; set; }
}

/// <summary>
/// Parameters for a tool call request.
/// </summary>
public class ToolCallParams
{
    /// <summary>
    /// Gets or sets the name of the tool to call.
    /// </summary>
    [JsonPropertyName("name")]
    public required string Name { get; set; }

    /// <summary>
    /// Gets or sets the tool arguments.
    /// </summary>
    [JsonPropertyName("arguments")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public JsonElement? Arguments { get; set; }
}

/// <summary>
/// Content part in a tool response.
/// </summary>
public class ContentPart
{
    /// <summary>
    /// Gets or sets the content type. Default is "text".
    /// </summary>
    [JsonPropertyName("type")]
    public string Type { get; set; } = "text";

    /// <summary>
    /// Gets or sets the text content.
    /// </summary>
    [JsonPropertyName("text")]
    public required string Text { get; set; }
}

/// <summary>
/// Response from a tool execution.
/// </summary>
public class ToolResponse
{
    /// <summary>
    /// Gets or sets the response content parts.
    /// </summary>
    [JsonPropertyName("content")]
    public List<ContentPart> Content { get; set; } = [];

    /// <summary>
    /// Gets or sets whether this response represents an error.
    /// </summary>
    [JsonPropertyName("isError")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public bool? IsError { get; set; }
}

/// <summary>
/// Response containing a list of tools.
/// </summary>
public class ToolsListResponse
{
    /// <summary>
    /// Gets or sets the available tools.
    /// </summary>
    [JsonPropertyName("tools")]
    public Tool[] Tools { get; set; } = Array.Empty<Tool>();
}
