using SharpMCP.Core.Tools;
using SharpMCP.Core.Protocol;
using SharpMCP.Core.Utils;
using McpServerTemplate.Services;
using Microsoft.Extensions.Logging;
using System.ComponentModel;
using System.Text.Json.Serialization;

namespace McpServerTemplate.Tools;

/// <summary>
/// A simple tool that greets users using dependency injection
/// </summary>
public class GreetingTool : McpToolBase<GreetingArgs>
{
    private readonly IGreetingService _greetingService;
    private readonly ILogger<GreetingTool> _logger;

    public GreetingTool(IGreetingService greetingService, ILogger<GreetingTool> logger)
    {
        _greetingService = greetingService;
        _logger = logger;
    }

    /// <inheritdoc />
    public override string Name => "greeting";

    /// <inheritdoc />
    public override string? Description => "Sends a personalized greeting message";

    /// <summary>
    /// Executes the greeting tool
    /// </summary>
    /// <param name="args">The greeting arguments</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A greeting message</returns>
    protected override async Task<ToolResponse> ExecuteAsync(GreetingArgs args, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Executing greeting tool for {Name}", args.Name);

        // Simulate some async work
        await Task.Delay(100, cancellationToken);

        var greeting = _greetingService.GenerateGreeting(args.Name, args.Style);
        
        return ToolResponseBuilder.Success(greeting);
    }
}

/// <summary>
/// Arguments for the greeting tool
/// </summary>
public class GreetingArgs
{
    /// <summary>
    /// The name of the person to greet
    /// </summary>
    [JsonPropertyName("name")]
    [JsonRequired]
    [Description("The name of the person to greet")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// The style of greeting
    /// </summary>
    [JsonPropertyName("style")]
    [Description("The style of greeting (formal, casual, excited)")]
    [DefaultValue("normal")]
    public string Style { get; set; } = "normal";
}
