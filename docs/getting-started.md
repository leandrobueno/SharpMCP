# Getting Started with SharpMCP

This guide will help you create your first MCP server using SharpMCP.

## Installation

Install the complete SharpMCP package:

```bash
dotnet add package SharpMCP.Server
```

This automatically includes `SharpMCP.Core` and `SharpMCP.Tools.Common`.

## Your First MCP Server

### 1. Create a New Project

```bash
dotnet new console -n MyMcpServer
cd MyMcpServer
dotnet add package SharpMCP.Server
```

### 2. Create a Simple Tool

```csharp
using SharpMCP.Core.Tools;
using SharpMCP.Core.Schema;

[McpTool("greet", Description = "Greets a person with a custom message")]
public class GreetTool : McpToolBase<GreetArgs>
{
    public override string Name => "greet";
    public override string? Description => "Greets a person with a custom message";

    protected override Task<ToolResponse> ExecuteAsync(GreetArgs args, CancellationToken ct)
    {
        var message = $"Hello, {args.Name}! {args.Message}";
        return Task.FromResult(Success(message));
    }
}

public class GreetArgs
{
    [JsonRequired]
    [JsonDescription("Name of the person to greet")]
    public string Name { get; set; } = "";

    [JsonDescription("Custom greeting message")]
    public string Message { get; set; } = "Nice to meet you!";
}
```

### 3. Create the Server

```csharp
using SharpMCP.Server;
using SharpMCP.Tools.Common;

class Program
{
    static async Task Main(string[] args)
    {
        var server = new McpServerBuilder()
            .WithName("MyMcpServer")
            .WithVersion("1.0.0")
            .AddTool(new GreetTool())
            .AddFileSystemTools() // Optional: add built-in file tools
            .Build();

        await server.RunAsync();
    }
}
```

### 4. Run Your Server

```bash
dotnet run
```

Your server will start and communicate via stdin/stdout using the MCP protocol.

## Using Built-in Tools

SharpMCP includes comprehensive file system tools:

```csharp
var server = new McpServerBuilder()
    .WithName("FileServer")
    .WithVersion("1.0.0")
    .AddFileSystemTools(allowedDirectories: [
        @"C:\MyProject", 
        @"C:\Data"
    ])
    .Build();
```

Available tools:
- `read_file` - Read file contents
- `write_file` - Write/create files
- `list_directory` - List directory contents
- `search_files` - Search with patterns
- `archive_operations` - ZIP operations
- And more...

## Testing Your Server

### Manual Testing

1. Run your server: `dotnet run`
2. Send JSON-RPC messages to stdin
3. Observe responses on stdout

### Using the Test Harness

```csharp
var server = new McpServerBuilder()
    .AddTool(new GreetTool())
    .Build();

var response = await server.ExecuteToolAsync("greet", new {
    name = "Alice",
    message = "Welcome!"
});

Console.WriteLine(response.Content[0].Text); // "Hello, Alice! Welcome!"
```

## Next Steps

- [Create custom tools](creating-tools.md)
- [Configure your server](configuration.md)
- [Security best practices](security.md)
- [Testing strategies](testing.md)
