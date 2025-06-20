using SharpMCP.Server;
using Microsoft.Extensions.Logging;
#if (useDI)
using Microsoft.Extensions.DependencyInjection;
using McpServerTemplate.Services;
using McpServerTemplate.Tools;
#else
using McpServerTemplate.Tools;
#endif

namespace McpServerTemplate;

/// <summary>
/// Main entry point for the MCP server
/// </summary>
class Program
{
    static async Task Main(string[] args)
    {
#if (useDI)
        // Configure services for dependency injection
        var services = new ServiceCollection();
        services.AddLogging(builder =>
        {
            builder
                .SetMinimumLevel(LogLevel.Information)
                .AddConsole();
        });
        
        // Add your services here
        services.AddSingleton<IGreetingService, GreetingService>();
        services.AddTransient<GreetingTool>();
        
        var serviceProvider = services.BuildServiceProvider();
        using var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
        var logger = loggerFactory.CreateLogger<Program>();
        
        try
        {
            logger.LogInformation("Starting McpServerTemplate MCP Server...");

            // Create and run an MCP server with DI
            await new McpServerBuilder()
                .WithServerInfo("McpServerTemplate", "1.0.0", "A simple MCP server created from template")
                .AddTool(serviceProvider.GetRequiredService<GreetingTool>())
                .UseStdio()
                .WithLoggerFactory(loggerFactory)
                .BuildAndRunAsync();
        }
        catch (Exception ex)
        {
            logger.LogCritical(ex, "Server failed to start");
            Environment.Exit(1);
        }
#else
        // Configure logging without DI
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

            // Create and run an MCP server
            await new McpServerBuilder()
                .WithServerInfo("McpServerTemplate", "1.0.0", "A simple MCP server created from template")
                .AddTool<GreetingTool>()
                .UseStdio()
                .WithLoggerFactory(loggerFactory)
                .BuildAndRunAsync();
        }
        catch (Exception ex)
        {
            logger.LogCritical(ex, "Server failed to start");
            Environment.Exit(1);
        }
#endif
    }
}
