# SharpMCP.Core API Reference

Core abstractions, interfaces, and protocol types for building MCP servers.

## Interfaces

### IMcpTool

Core interface for all MCP tools.

```csharp
public interface IMcpTool
{
    string Name { get; }
    string? Description { get; }
    Task<ToolResponse> ExecuteAsync(JsonElement? arguments, CancellationToken cancellationToken = default);
    JsonElement GetInputSchema();
}
```

### IMcpServer

Main server interface for MCP server implementations.

```csharp
public interface IMcpServer
{
    ServerInfo ServerInfo { get; }
    IReadOnlyList<Tool> Tools { get; }
    
    event EventHandler<ServerLifecycleEventArgs>? Started;
    event EventHandler<ServerLifecycleEventArgs>? Stopped;
    event EventHandler<ToolExecutionEventArgs>? ToolExecuted;
    
    void RegisterTool(IMcpTool tool);
    Task<ToolResponse> ExecuteToolAsync(string toolName, JsonElement? arguments, CancellationToken cancellationToken = default);
    Task StartAsync(CancellationToken cancellationToken = default);
    Task StopAsync(CancellationToken cancellationToken = default);
    Task RunAsync(CancellationToken cancellationToken = default);
}
```

### IMcpTransport

Transport layer abstraction for different communication methods.

```csharp
public interface IMcpTransport : IDisposable
{
    bool IsConnected { get; }
    event EventHandler<JsonRpcRequest>? RequestReceived;
    event EventHandler? Disconnected;
    
    Task ConnectAsync(CancellationToken cancellationToken = default);
    Task DisconnectAsync(CancellationToken cancellationToken = default);
    Task SendAsync(JsonRpcMessage message, CancellationToken cancellationToken = default);
}
```

### IMcpServerBuilder

Fluent builder interface for server configuration.

```csharp
public interface IMcpServerBuilder
{
    IMcpServerBuilder WithName(string name);
    IMcpServerBuilder WithVersion(string version);
    IMcpServerBuilder WithDescription(string description);
    IMcpServerBuilder WithOptions(McpServerOptions options);
    IMcpServerBuilder AddTool(IMcpTool tool);
    IMcpServer Build();
}
```

## Base Classes

### McpToolBase<TArgs>

Generic base class for strongly-typed tools.

```csharp
public abstract class McpToolBase<TArgs> : IMcpTool where TArgs : class, new()
{
    public abstract string Name { get; }
    public abstract string? Description { get; }
    
    public virtual async Task<ToolResponse> ExecuteAsync(JsonElement? arguments, CancellationToken cancellationToken = default);
    public virtual JsonElement GetInputSchema();
    
    protected abstract Task<ToolResponse> ExecuteAsync(TArgs args, CancellationToken cancellationToken);
    protected virtual string? ValidateArguments(TArgs args);
    
    protected static ToolResponse Success(string text);
    protected static ToolResponse Success(ContentPart content);
    protected static ToolResponse Error(string error);
}
```

### McpToolBase

Base class for tools without arguments.

```csharp
public abstract class McpToolBase : IMcpTool
{
    public abstract string Name { get; }
    public abstract string? Description { get; }
    
    public virtual Task<ToolResponse> ExecuteAsync(JsonElement? arguments, CancellationToken cancellationToken = default);
    public virtual JsonElement GetInputSchema();
    
    protected abstract Task<ToolResponse> ExecuteAsync(CancellationToken cancellationToken);
    
    protected static ToolResponse Success(string text);
    protected static ToolResponse Success(ContentPart content);
    protected static ToolResponse Error(string error);
}
```

## Protocol Types

### JsonRpcRequest

Represents a JSON-RPC request message.

```csharp
public class JsonRpcRequest : JsonRpcMessage
{
    public string Method { get; set; } = "";
    public JsonElement? Params { get; set; }
}
```

### JsonRpcResponse

Represents a JSON-RPC response message.

```csharp
public class JsonRpcResponse : JsonRpcMessage
{
    public JsonElement? Result { get; set; }
    public JsonRpcError? Error { get; set; }
}
```

### ServerInfo

Contains server metadata and capabilities.

```csharp
public class ServerInfo
{
    public string Name { get; set; } = "";
    public string Version { get; set; } = "";
    public string? Description { get; set; }
    public ServerCapabilities Capabilities { get; set; } = new();
}
```

### Tool

Represents a tool definition in the MCP protocol.

```csharp
public class Tool
{
    public string Name { get; set; } = "";
    public string? Description { get; set; }
    public JsonElement InputSchema { get; set; }
}
```

### ToolResponse

Represents the response from tool execution.

```csharp
public class ToolResponse
{
    public List<ContentPart> Content { get; set; } = [];
    public bool IsError { get; set; }
    
    public static ToolResponse FromContent(string text);
    public static ToolResponse FromError(string error);
}
```

### ContentPart

Represents a piece of content in responses.

```csharp
public class ContentPart
{
    public string Type { get; set; } = "text";
    public string Text { get; set; } = "";
}
```

## Attributes

### McpToolAttribute

Decorates classes to mark them as MCP tools.

```csharp
[AttributeUsage(AttributeTargets.Class)]
public class McpToolAttribute : Attribute
{
    public string Name { get; }
    public string? Description { get; set; }
    
    public McpToolAttribute(string name);
}
```

### JsonRequiredAttribute

Marks properties as required in JSON schema.

```csharp
[AttributeUsage(AttributeTargets.Property)]
public class JsonRequiredAttribute : Attribute
{
}
```

### JsonDescriptionAttribute

Provides descriptions for JSON schema properties.

```csharp
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Class)]
public class JsonDescriptionAttribute : Attribute
{
    public string Description { get; }
    
    public JsonDescriptionAttribute(string description);
}
```

### JsonStringConstraintsAttribute

Defines validation constraints for string properties.

```csharp
[AttributeUsage(AttributeTargets.Property)]
public class JsonStringConstraintsAttribute : Attribute
{
    public int MinLength { get; set; } = -1;
    public int MaxLength { get; set; } = -1;
    public string? Pattern { get; set; }
    public string? Format { get; set; }
}
```

### JsonNumberConstraintsAttribute

Defines validation constraints for numeric properties.

```csharp
[AttributeUsage(AttributeTargets.Property)]
public class JsonNumberConstraintsAttribute : Attribute
{
    public double Minimum { get; set; }
    public double Maximum { get; set; }
    public bool HasMinimum { get; set; }
    public bool HasMaximum { get; set; }
}
```

### JsonArrayConstraintsAttribute

Defines validation constraints for array properties.

```csharp
[AttributeUsage(AttributeTargets.Property)]
public class JsonArrayConstraintsAttribute : Attribute
{
    public int MinItems { get; set; } = -1;
    public int MaxItems { get; set; } = -1;
    public bool UniqueItems { get; set; }
}
```

### JsonEnumAttribute

Defines allowed enumeration values.

```csharp
[AttributeUsage(AttributeTargets.Property)]
public class JsonEnumAttribute : Attribute
{
    public object[] Values { get; }
    
    public JsonEnumAttribute(params object[] values);
}
```

## Utilities

### ToolResponseBuilder

Fluent builder for creating tool responses.

```csharp
public class ToolResponseBuilder
{
    public ToolResponseBuilder WithContent(string text);
    public ToolResponseBuilder WithTypedContent(string type, string text);
    public ToolResponseBuilder WithError(string error);
    public ToolResponse Build();
}
```

### JsonSchemaGenerator

Generates JSON schemas from C# types.

```csharp
public static class JsonSchemaGenerator
{
    public static JsonSchema GenerateSchema<T>();
    public static JsonSchema GenerateSchema(Type type);
}
```

## Configuration

### McpServerOptions

Configuration options for MCP servers.

```csharp
public class McpServerOptions
{
    public string Name { get; set; } = "";
    public string Version { get; set; } = "1.0.0";
    public string? Description { get; set; }
    public int MaxConcurrentTools { get; set; } = 5;
    public TimeSpan RequestTimeout { get; set; } = TimeSpan.FromSeconds(30);
    public bool EnableDetailedErrors { get; set; } = false;
}
```

## Exceptions

### McpToolException

Exception specific to tool execution errors.

```csharp
public class McpToolException : Exception
{
    public McpToolException(string message);
    public McpToolException(string message, Exception innerException);
}
```

### McpTransportException

Exception for transport-related errors.

```csharp
public class McpTransportException : Exception
{
    public McpTransportException(string message);
    public McpTransportException(string message, Exception innerException);
}
```

## Event Args

### ServerLifecycleEventArgs

Event arguments for server lifecycle events.

```csharp
public class ServerLifecycleEventArgs : EventArgs
{
    public DateTime Timestamp { get; }
    public ServerInfo ServerInfo { get; }
}
```

### ToolExecutionEventArgs

Event arguments for tool execution events.

```csharp
public class ToolExecutionEventArgs : EventArgs
{
    public string ToolName { get; }
    public TimeSpan Duration { get; }
    public bool Success { get; }
    public string? Error { get; }
    public DateTime Timestamp { get; }
}
```
