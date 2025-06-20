using SharpMCP.Server;
using SharpMCP.Templates.Server.Tools;
using Microsoft.Extensions.Logging;

// Create and configure the MCP server
var server = new McpServerBuilder()
    .WithName("SharpMCP.Templates.Server")
    .WithVersion("1.0.0")
    .WithDescription("A simple MCP server created from the SharpMCP template")
    .ConfigureLogging(logging =>
    {
        logging.SetMinimumLevel(LogLevel.Information);
        logging.AddConsole();
    })
    .AddTool<GreetingTool>()
    .AddTool<DateTimeTool>()
    .Build();

// Run the server
await server.RunAsync();
