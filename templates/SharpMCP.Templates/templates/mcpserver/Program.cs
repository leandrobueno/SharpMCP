using SharpMCP;
using SharpMCP.Server;
using Microsoft.Extensions.Logging;

namespace McpServerTemplate;

/// <summary>
/// Main entry point for the MCP server
/// </summary>
class Program
{
    static async Task Main(string[] args)
    {
        // Configure logging
        using var loggerFactory = LoggerFactory.Create(builder =>
        {
            builder
                .SetMinimumLevel(LogLevel.Information)
                .AddConsole();
        });

        var logger = loggerFactory.CreateLogger<Program>();

        try
        {
            logger.LogInformation("Starting McpServerTemplate MCP Server...");

            // Build the server
            var server = new McpServerBuilder()
                .WithName("McpServerTemplate")
                .WithVersion("1.0.0")
                .WithDescription("A simple MCP server created from template")
                .AddTool<GreetingTool>()
                .UseStdioTransport()
                .WithLoggerFactory(loggerFactory)
                .Build();

            // Run the server
            await server.RunAsync();
        }
        catch (Exception ex)
        {
            logger.LogCritical(ex, "Server failed to start");
            Environment.Exit(1);
        }
    }
}
