using SharpMCP;
using SharpMCP.Server;
using Microsoft.Extensions.Logging;

namespace McpToolSetTemplate;

/// <summary>
/// Main entry point for the MCP tool collection server
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
            logger.LogInformation("Starting McpToolSetTemplate MCP Server...");

            // Build the server with multiple tools
            var serverBuilder = new McpServerBuilder()
                .WithName("McpToolSetTemplate")
                .WithVersion("1.0.0")
                .WithDescription("A collection of MCP tools for various operations")
                .UseStdioTransport()
                .WithLoggerFactory(loggerFactory);

            // Register all tools
            serverBuilder
                .AddTool<TextProcessingTool>()
                .AddTool<CalculatorTool>()
                .AddTool<DataTransformTool>()
                .AddTool<ValidationTool>()
                .AddTool<ReportGeneratorTool>();

            var server = serverBuilder.Build();

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
