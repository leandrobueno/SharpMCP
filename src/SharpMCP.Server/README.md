# SharpMCP.Server

Server implementation for building MCP servers with SharpMCP.

## Features

- **McpServerBase** - Abstract base class with built-in JSON-RPC message handling
- **StdioTransport** - Standard input/output transport for MCP communication
- **McpToolBase<TArgs>** - Generic base class for tools with automatic argument parsing
- **McpServerBuilder** - Fluent API for server configuration
- **JsonSchemaGenerator** - Automatic JSON Schema generation from C# types

## Usage

### Simple Server

```csharp
using SharpMCP.Server;
using SharpMCP.Server.Transport;
using Microsoft.Extensions.Logging;

// Create and configure server
var server = new McpServerBuilder()
    .WithServerInfo("MyServer", "1.0.0", "My MCP Server")
    .WithStdioTransport()
    .AddTool<MyCustomTool>()
    .Build();

// Start server
await server.StartAsync();

// Keep server running
await Task.Delay(Timeout.Infinite);
```

### Creating Tools

```csharp
using SharpMCP.Server.Tools;

// Tool with arguments
public class CalculatorTool : McpToolBase<CalculatorArgs>
{
    public override string Name => "calculator";
    public override string Description => "Performs calculations";
    
    protected override async Task<ToolResponse> ExecuteAsync(
        CalculatorArgs args, 
        CancellationToken cancellationToken)
    {
        var result = args.Operation switch
        {
            "add" => args.A + args.B,
            "subtract" => args.A - args.B,
            "multiply" => args.A * args.B,
            "divide" => args.B != 0 ? args.A / args.B : double.NaN,
            _ => double.NaN
        };
        
        return Success($"Result: {result}");
    }
}

public class CalculatorArgs
{
    [JsonRequired]
    [JsonDescription("The operation to perform")]
    [JsonEnum("add", "subtract", "multiply", "divide")]
    public string Operation { get; set; } = "add";
    
    [JsonRequired]
    [JsonDescription("First number")]
    public double A { get; set; }
    
    [JsonRequired]
    [JsonDescription("Second number")]
    public double B { get; set; }
}

// Tool without arguments
public class PingTool : McpToolBase
{
    public override string Name => "ping";
    public override string Description => "Simple ping tool";
    
    protected override Task<ToolResponse> ExecuteAsync(CancellationToken cancellationToken)
    {
        return Task.FromResult(Success("Pong!"));
    }
}
```

### Custom Server Implementation

```csharp
public class MyCustomServer : McpServerBase
{
    public MyCustomServer(McpServerOptions options, ILogger<MyCustomServer>? logger = null)
        : base(options, logger)
    {
    }
    
    protected override async Task<JsonRpcResponse> HandleRequestAsync(
        JsonRpcRequest request, 
        CancellationToken cancellationToken)
    {
        // Add custom request handling
        if (request.Method == "custom/method")
        {
            return new JsonRpcResponse
            {
                Id = request.Id,
                Result = JsonSerializer.SerializeToElement(new { custom = "response" })
            };
        }
        
        // Fall back to base implementation
        return await base.HandleRequestAsync(request, cancellationToken);
    }
}

// Use custom server
var server = new McpServerBuilder()
    .WithServerInfo("CustomServer", "1.0.0")
    .UseServer<MyCustomServer>()
    .Build();
```

## Dependencies

- SharpMCP.Core
- Microsoft.Extensions.Logging
- Microsoft.Extensions.DependencyInjection.Abstractions
- Microsoft.Extensions.Options
