using System.Text.Json.Serialization;
using SharpMCP.Core.Protocol;
using SharpMCP.Core.Schema;

namespace SharpMCP.Core.Utils;

/// <summary>
/// JSON serialization context for MCP types.
/// Enables source generation for better performance.
/// </summary>
[JsonSerializable(typeof(JsonRpcMessage))]
[JsonSerializable(typeof(JsonRpcRequest))]
[JsonSerializable(typeof(JsonRpcResponse))]
[JsonSerializable(typeof(JsonRpcError))]
[JsonSerializable(typeof(ServerInfo))]
[JsonSerializable(typeof(ServerMetadata))]
[JsonSerializable(typeof(ServerCapabilities))]
[JsonSerializable(typeof(ToolsCapability))]
[JsonSerializable(typeof(ResourcesCapability))]
[JsonSerializable(typeof(PromptsCapability))]
[JsonSerializable(typeof(Tool))]
[JsonSerializable(typeof(ToolCallParams))]
[JsonSerializable(typeof(ContentPart))]
[JsonSerializable(typeof(ToolResponse))]
[JsonSerializable(typeof(ToolsListResponse))]
[JsonSerializable(typeof(JsonSchema))]
[JsonSerializable(typeof(Dictionary<string, object>))]
[JsonSerializable(typeof(List<string>))]
[JsonSerializable(typeof(string[]))]
[JsonSerializable(typeof(object[]))]
public partial class McpJsonContext : JsonSerializerContext
{
}
