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

            // Create and run an MCP server with multiple tools
            await new McpServerBuilder()
                .WithServerInfo("McpToolSetTemplate", "1.0.0", "A collection of MCP tools for various operations")
                .UseStdio()
                .WithLoggerFactory(loggerFactory)
                // Register all tools
                .AddTool<TextProcessingTool>()
                .AddTool<CalculatorTool>()
                .AddTool<DataTransformTool>()
                .AddTool<ValidationTool>()
                .AddTool<ReportGeneratorTool>()
                .BuildAndRunAsync();
        }
        catch (Exception ex)
        {
            logger.LogCritical(ex, "Server failed to start");
            Environment.Exit(1);
        }
    }
}
