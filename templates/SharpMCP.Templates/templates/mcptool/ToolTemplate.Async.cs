using SharpMCP.Core.Tools;
using SharpMCP.Core.Protocol;
using SharpMCP.Core.Utils;
using System.ComponentModel;
using System.Text.Json.Serialization;

namespace ToolNamespace;

/// <summary>
/// TOOL_DESCRIPTION
/// </summary>
public class ToolTemplate : McpToolBase<ToolTemplateArgs>
{
    /// <inheritdoc />
    public override string Name => "TOOL_ID";

    /// <inheritdoc />
    public override string? Description => "TOOL_DESCRIPTION";

    /// <summary>
    /// Executes the tool operation
    /// </summary>
    /// <param name="args">The tool arguments</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The tool response</returns>
    protected override async Task<ToolResponse> ExecuteAsync(ToolTemplateArgs args, CancellationToken cancellationToken)
    {
        try
        {
            // TODO: Implement your tool logic here
            await Task.Delay(100, cancellationToken); // Remove this - just simulating async work
            
            // Example: Process the input and return a result
            var result = $"Processed: {args.Input}";
            
            return ToolResponseBuilder.Success(result);
        }
        catch (Exception ex)
        {
            return ToolResponseBuilder.Error($"Error executing tool: {ex.Message}");
        }
    }
}

/// <summary>
/// Arguments for ToolTemplate
/// </summary>
public class ToolTemplateArgs
{
    /// <summary>
    /// The input to process
    /// </summary>
    [JsonPropertyName("input")]
    [JsonRequired]
    [Description("The input value to process")]
    public string Input { get; set; } = string.Empty;

    /// <summary>
    /// Optional configuration value
    /// </summary>
    [JsonPropertyName("options")]
    [Description("Optional configuration for the operation")]
    public ToolOptions? Options { get; set; }
}

/// <summary>
/// Optional configuration for the tool
/// </summary>
public class ToolOptions
{
    /// <summary>
    /// Whether to use verbose output
    /// </summary>
    [JsonPropertyName("verbose")]
    [Description("Enable verbose output")]
    [DefaultValue(false)]
    public bool Verbose { get; set; }

    /// <summary>
    /// Maximum items to process
    /// </summary>
    [JsonPropertyName("maxItems")]
    [Description("Maximum number of items to process")]
    [DefaultValue(10)]
    public int MaxItems { get; set; } = 10;
}
