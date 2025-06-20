using SharpMCP;
using SharpMCP.Core.Tools;
using SharpMCP.Core.Protocol;
using SharpMCP.Core.Utilities;
using System.ComponentModel;
using System.Text.Json.Serialization;

namespace ToolNamespace;

/// <summary>
/// TOOL_DESCRIPTION
/// </summary>
[McpTool("TOOL_ID", "TOOL_DESCRIPTION")]
public class ToolTemplate : McpToolBase<ToolTemplateArgs>
{
    /// <summary>
    /// Executes the tool operation
    /// </summary>
    /// <param name="args">The tool arguments</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The tool response</returns>
    protected override Task<ToolResponse> ExecuteAsync(ToolTemplateArgs args, CancellationToken cancellationToken)
    {
        try
        {
            // TODO: Implement your tool logic here
            
            // Example: Process the input and return a result
            var result = $"Processed: {args.Input}";
            
            return Task.FromResult(ToolResponseBuilder.Success(result));
        }
        catch (Exception ex)
        {
            return Task.FromResult(ToolResponseBuilder.Error($"Error executing tool: {ex.Message}"));
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
