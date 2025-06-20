using SharpMCP;
using SharpMCP.Server;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace McpServerTemplate;

/// <summary>
/// Main entry point for the MCP server with dependency injection
/// </summary>
class Program
{
    static async Task Main(string[] args)
    {
        // Configure services
        var services = new ServiceCollection();
        ConfigureServices(services);

        var serviceProvider = services.BuildServiceProvider();
        var logger = serviceProvider.GetRequiredService<ILogger<Program>>();

        try
        {
            logger.LogInformation("Starting McpServerTemplate MCP Server with DI...");

            // Build the server
            var server = new McpServerBuilder()
                .WithName("McpServerTemplate")
                .WithVersion("1.0.0")
                .WithDescription("A simple MCP server created from template with DI support")
                .AddTool<GreetingTool>()
                .UseStdioTransport()
                .WithServiceProvider(serviceProvider)
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

    private static void ConfigureServices(IServiceCollection services)
    {
        // Configure logging
        services.AddLogging(builder =>
        {
            builder
                .SetMinimumLevel(LogLevel.Information)
                .AddConsole();
        });

        // Add your services here
        services.AddSingleton<IGreetingService, GreetingService>();
        
        // Register tools
        services.AddTransient<GreetingTool>();
    }
}
