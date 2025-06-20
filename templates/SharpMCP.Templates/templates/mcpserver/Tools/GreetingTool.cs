using SharpMCP;
using SharpMCP.Core.Tools;
using SharpMCP.Core.Protocol;
using SharpMCP.Core.Utilities;
using System.ComponentModel;
using System.Text.Json.Serialization;

namespace McpServerTemplate.Tools;

/// <summary>
/// A simple tool that greets users
/// </summary>
[McpTool("greeting", "Sends a personalized greeting message")]
public class GreetingTool : McpToolBase<GreetingArgs>
{
    /// <summary>
    /// Executes the greeting tool
    /// </summary>
    /// <param name="args">The greeting arguments</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A greeting message</returns>
    protected override async Task<ToolResponse> ExecuteAsync(GreetingArgs args, CancellationToken cancellationToken)
    {
        // Simulate some async work
        await Task.Delay(100, cancellationToken);

        var greeting = args.Style switch
        {
            "formal" => $"Good day, {args.Name}. How may I assist you today?",
            "casual" => $"Hey {args.Name}! What's up?",
            "excited" => $"Hello {args.Name}! 🎉 Great to see you!",
            _ => $"Hello, {args.Name}!"
        };

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
