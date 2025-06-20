# MCP-Safe Logging in SharpMCP

## Overview

The Model Context Protocol (MCP) uses stdin/stdout for JSON-RPC communication. This means that **all logging must go to stderr** to avoid corrupting the protocol messages.

SharpMCP automatically handles this for you when using stdio transport.

## Automatic Behavior

When you call `.UseStdio()` without providing a logger factory, SharpMCP automatically configures MCP-safe logging:

```csharp
await new McpServerBuilder()
    .WithServerInfo("MyServer", "1.0.0")
    .UseStdio()  // Automatically enables MCP-safe logging to stderr
    .AddTool<MyTool>()
    .BuildAndRunAsync();
```

This automatically:
- ✅ Writes all logs to **stderr** (not stdout)
- ✅ Uses `Information` log level by default
- ✅ Formats logs with timestamps and categories
- ✅ Keeps stdout clean for JSON-RPC communication

## Manual Configuration

If you want to customize the logging behavior:

### Option 1: Use MCP-Safe Console Logging
```csharp
using var loggerFactory = LoggerFactory.Create(builder =>
{
    builder.AddMcpConsole(LogLevel.Debug);  // MCP-safe console logging
});

await new McpServerBuilder()
    .WithServerInfo("MyServer", "1.0.0")  
    .WithLoggerFactory(loggerFactory)  // Your custom factory
    .UseStdio()
    .AddTool<MyTool>()
    .BuildAndRunAsync();
```

### Option 2: Use the Builder Method
```csharp
await new McpServerBuilder()
    .WithServerInfo("MyServer", "1.0.0")
    .WithMcpSafeLogging(LogLevel.Debug)  // Explicit MCP-safe logging
    .UseStdio()
    .AddTool<MyTool>()
    .BuildAndRunAsync();
```

### Option 3: Custom Logger Factory
```csharp
using var loggerFactory = LoggerFactory.Create(builder =>
{
    // IMPORTANT: Never use AddConsole() - it writes to stdout!
    // builder.AddConsole();  // ❌ This breaks MCP communication
    
    builder.AddMcpConsole();  // ✅ Use this instead
    // Or configure your own stderr-based logging
});
```

## ⚠️ Common Pitfalls

### DON'T use standard console logging:
```csharp
// ❌ This writes to stdout and breaks MCP communication
using var loggerFactory = LoggerFactory.Create(builder =>
{
    builder.AddConsole();  // This writes to stdout - BAD!
});
```

### DON'T write directly to stdout:
```csharp
// ❌ Never do this in MCP servers
Console.WriteLine("Some debug info");  // This breaks JSON-RPC
```

### DO use stderr for any custom output:
```csharp
// ✅ This is safe
Console.Error.WriteLine("Debug info goes to stderr");
```

## Log Output Format

MCP-safe logs are formatted as:
```
[2024-06-18 13:45:23.123] [INFO ] SharpMCP...Server: Starting MCP server
[2024-06-18 13:45:23.124] [INFO ] SharpMCP...Server: Registered tool: read_file
```

## Integration with Other Loggers

You can combine MCP-safe logging with other providers:

```csharp
using var loggerFactory = LoggerFactory.Create(builder =>
{
    builder
        .AddMcpConsole(LogLevel.Information)  // Console to stderr
        .AddFile("logs/server.log")           // File logging
        .AddApplicationInsights();            // Cloud logging
});
```

## Why This Matters

MCP clients expect **only JSON-RPC messages** on stdout. Any other output causes parsing errors like:

```
Unexpected token 'i', "info: Starting"... is not valid JSON
```

By ensuring all logging goes to stderr, your MCP server will work correctly with any MCP client (Claude Desktop, MCP Inspector, etc.).
