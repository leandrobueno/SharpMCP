# SharpMCP.Server API Reference

Server implementation and transport components for MCP servers.

## Core Classes

### McpServerBuilder

Fluent builder for creating and configuring MCP servers.

```csharp
public class McpServerBuilder : IMcpServerBuilder
{
    public McpServerBuilder WithName(string name);
    public McpServerBuilder WithVersion(string version);
    public McpServerBuilder WithDescription(string description);
    public McpServerBuilder WithOptions(McpServerOptions options);
    public McpServerBuilder WithTransport(IMcpTransport transport);
    public McpServerBuilder WithLogging(Action<ILoggingBuilder> configure);
    
    public McpServerBuilder AddTool(IMcpTool tool);
    public McpServerBuilder AddTool<T>() where T : IMcpTool, new();
    public McpServerBuilder AddTools(IEnumerable<IMcpTool> tools);
    
    public IMcpServer Build();
    public Task<IMcpServer> BuildAndRunAsync(CancellationToken cancellationToken = default);
}
```

### DefaultMcpServer

Default implementation of IMcpServer.

```csharp
internal class DefaultMcpServer : IMcpServer
{
    public ServerInfo ServerInfo { get; }
    public IReadOnlyList<Tool> Tools { get; }
    
    public event EventHandler<ServerLifecycleEventArgs>? Started;
    public event EventHandler<ServerLifecycleEventArgs>? Stopped;
    public event EventHandler<ToolExecutionEventArgs>? ToolExecuted;
    
    public void RegisterTool(IMcpTool tool);
    public Task<ToolResponse> ExecuteToolAsync(string toolName, JsonElement? arguments, CancellationToken cancellationToken = default);
    public Task StartAsync(CancellationToken cancellationToken = default);
    public Task StopAsync(CancellationToken cancellationToken = default);
    public Task RunAsync(CancellationToken cancellationToken = default);
    
    public Task<JsonRpcResponse> HandleRequestAsync(JsonRpcRequest request, CancellationToken cancellationToken = default);
}
```

## Transport Layer

### StdioTransport

Standard input/output transport for MCP communication.

```csharp
public class StdioTransport : McpTransportBase
{
    public StdioTransport();
    public StdioTransport(TextReader input, TextWriter output);
    
    public override bool IsConnected { get; }
    
    public override Task ConnectAsync(CancellationToken cancellationToken = default);
    public override Task DisconnectAsync(CancellationToken cancellationToken = default);
    public override Task SendAsync(JsonRpcMessage message, CancellationToken cancellationToken = default);
    
    protected override void Dispose(bool disposing);
}
```

### McpTransportBase

Abstract base class for transport implementations.

```csharp
public abstract class McpTransportBase : IMcpTransport
{
    public abstract bool IsConnected { get; }
    
    public event EventHandler<JsonRpcRequest>? RequestReceived;
    public event EventHandler? Disconnected;
    
    public abstract Task ConnectAsync(CancellationToken cancellationToken = default);
    public abstract Task DisconnectAsync(CancellationToken cancellationToken = default);
    public abstract Task SendAsync(JsonRpcMessage message, CancellationToken cancellationToken = default);
    
    protected virtual void OnRequestReceived(JsonRpcRequest request);
    protected virtual void OnDisconnected();
    
    public void Dispose();
    protected virtual void Dispose(bool disposing);
}
```

## Message Handling

### JsonRpcHandler

Handles JSON-RPC message processing.

```csharp
public class JsonRpcHandler
{
    public Task<JsonRpcResponse> HandleAsync(JsonRpcRequest request, CancellationToken cancellationToken = default);
    
    public void RegisterMethod(string method, Func<JsonElement?, CancellationToken, Task<JsonElement?>> handler);
    public void RegisterMethod<TRequest>(string method, Func<TRequest, CancellationToken, Task<JsonElement?>> handler);
    public void RegisterMethod<TRequest, TResponse>(string method, Func<TRequest, CancellationToken, Task<TResponse>> handler);
}
```

## Server Operations

### ToolRegistry

Manages registered tools and their metadata.

```csharp
public class ToolRegistry
{
    public IReadOnlyList<Tool> Tools { get; }
    public IReadOnlyDictionary<string, IMcpTool> ToolMap { get; }
    
    public void RegisterTool(IMcpTool tool);
    public void RegisterTools(IEnumerable<IMcpTool> tools);
    public IMcpTool? GetTool(string name);
    public bool HasTool(string name);
    public Tool GetToolDefinition(string name);
    public List<Tool> GetAllToolDefinitions();
}
```

### ToolExecutor

Executes tools with proper error handling and monitoring.

```csharp
public class ToolExecutor
{
    public event EventHandler<ToolExecutionEventArgs>? ToolExecuted;
    
    public async Task<ToolResponse> ExecuteAsync(IMcpTool tool, JsonElement? arguments, CancellationToken cancellationToken = default);
    public async Task<ToolResponse> ExecuteAsync(string toolName, JsonElement? arguments, ToolRegistry registry, CancellationToken cancellationToken = default);
}
```

## Method Handlers

### StandardMethods

Built-in MCP method implementations.

```csharp
public static class StandardMethods
{
    // Server information
    public static Task<JsonElement?> HandleInitializeAsync(InitializeRequest request, CancellationToken cancellationToken);
    public static Task<JsonElement?> HandlePingAsync(CancellationToken cancellationToken);
    
    // Tool operations
    public static Task<JsonElement?> HandleListToolsAsync(ToolRegistry registry, CancellationToken cancellationToken);
    public static Task<JsonElement?> HandleCallToolAsync(CallToolRequest request, ToolExecutor executor, ToolRegistry registry, CancellationToken cancellationToken);
    
    // Capability management
    public static Task<JsonElement?> HandleNotificationsAsync(JsonElement? parameters, CancellationToken cancellationToken);
}
```

## Request/Response Types

### InitializeRequest

Request for server initialization.

```csharp
public class InitializeRequest
{
    public string ProtocolVersion { get; set; } = "";
    public ClientCapabilities Capabilities { get; set; } = new();
    public ClientInfo ClientInfo { get; set; } = new();
}
```

### CallToolRequest

Request for tool execution.

```csharp
public class CallToolRequest
{
    public string Name { get; set; } = "";
    public JsonElement? Arguments { get; set; }
}
```

### ListToolsResponse

Response containing available tools.

```csharp
public class ListToolsResponse
{
    public List<Tool> Tools { get; set; } = [];
}
```

## Configuration Extensions

### ServerBuilderExtensions

Extension methods for server configuration.

```csharp
public static class ServerBuilderExtensions
{
    public static McpServerBuilder AddFileSystemTools(this McpServerBuilder builder, List<string>? allowedDirectories = null);
    public static McpServerBuilder AddCustomTools(this McpServerBuilder builder, Assembly assembly);
    public static McpServerBuilder WithStdioTransport(this McpServerBuilder builder);
    public static McpServerBuilder WithConsoleLogging(this McpServerBuilder builder);
}
```

## Server Lifecycle

### ServerState

Enumeration of server states.

```csharp
public enum ServerState
{
    Stopped,
    Starting,
    Running,
    Stopping
}
```

### ServerLifecycleManager

Manages server lifecycle and state transitions.

```csharp
public class ServerLifecycleManager
{
    public ServerState State { get; }
    
    public event EventHandler<ServerLifecycleEventArgs>? StateChanged;
    
    public async Task StartAsync(CancellationToken cancellationToken = default);
    public async Task StopAsync(CancellationToken cancellationToken = default);
    public async Task RestartAsync(CancellationToken cancellationToken = default);
}
```

## Error Handling

### ServerErrorHandler

Centralized error handling for server operations.

```csharp
public class ServerErrorHandler
{
    public JsonRpcResponse HandleException(Exception exception, string? requestId = null);
    public JsonRpcError CreateError(int code, string message, JsonElement? data = null);
    
    public static JsonRpcError InvalidRequest(string message);
    public static JsonRpcError MethodNotFound(string method);
    public static JsonRpcError InvalidParams(string message);
    public static JsonRpcError InternalError(string message);
}
```

## Constants

### JsonRpcErrorCodes

Standard JSON-RPC error codes.

```csharp
public static class JsonRpcErrorCodes
{
    public const int ParseError = -32700;
    public const int InvalidRequest = -32600;
    public const int MethodNotFound = -32601;
    public const int InvalidParams = -32602;
    public const int InternalError = -32603;
    
    // MCP-specific codes
    public const int ToolNotFound = -32000;
    public const int ToolExecutionError = -32001;
    public const int InvalidTool = -32002;
}
```

### McpMethods

Standard MCP method names.

```csharp
public static class McpMethods
{
    public const string Initialize = "initialize";
    public const string Ping = "ping";
    public const string ListTools = "tools/list";
    public const string CallTool = "tools/call";
    public const string Notifications = "notifications/message";
}
```

## Performance

### ServerMetrics

Performance and usage metrics.

```csharp
public class ServerMetrics
{
    public int TotalRequests { get; }
    public int SuccessfulRequests { get; }
    public int FailedRequests { get; }
    public TimeSpan AverageResponseTime { get; }
    public int ConcurrentRequests { get; }
    public DateTime LastRequestTime { get; }
    
    public void RecordRequest(TimeSpan duration, bool success);
    public void Reset();
}
```

### ConcurrencyManager

Manages concurrent tool execution.

```csharp
public class ConcurrencyManager
{
    public int MaxConcurrentOperations { get; }
    public int CurrentOperations { get; }
    
    public async Task<T> ExecuteAsync<T>(Func<CancellationToken, Task<T>> operation, CancellationToken cancellationToken = default);
    public async Task ExecuteAsync(Func<CancellationToken, Task> operation, CancellationToken cancellationToken = default);
}
```
