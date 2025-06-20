using SharpMCP.Core.Tools;
using SharpMCP.Core.Protocol;
using SharpMCP.Core.Utils;
using System.ComponentModel;
using System.Text.Json.Serialization;
using System.Text;

namespace McpToolSetTemplate.Tools;

/// <summary>
/// Tool for text processing operations
/// </summary>
public class TextProcessingTool : McpToolBase<TextProcessingArgs>
{
    /// <inheritdoc />
    public override string Name => "text_processing";

    /// <inheritdoc />
    public override string? Description => "Performs various text processing operations";

    protected override async Task<ToolResponse> ExecuteAsync(TextProcessingArgs args, CancellationToken cancellationToken)
    {
        await Task.Delay(50, cancellationToken); // Simulate work

        var result = args.Operation switch
        {
            "uppercase" => args.Text.ToUpper(),
            "lowercase" => args.Text.ToLower(),
            "reverse" => new string(args.Text.Reverse().ToArray()),
            "wordcount" => CountWords(args.Text).ToString(),
            "trim" => args.Text.Trim(),
            "base64encode" => Convert.ToBase64String(Encoding.UTF8.GetBytes(args.Text)),
            "base64decode" => TryBase64Decode(args.Text),
            _ => throw new ArgumentException($"Unknown operation: {args.Operation}")
        };

        return ToolResponseBuilder.Success(result);
    }

    private static int CountWords(string text)
    {
        return text.Split(new[] { ' ', '\t', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries).Length;
    }

    private static string TryBase64Decode(string encoded)
    {
        try
        {
            var bytes = Convert.FromBase64String(encoded);
            return Encoding.UTF8.GetString(bytes);
        }
        catch
        {
            throw new ArgumentException("Invalid base64 string");
        }
    }
}

/// <summary>
/// Arguments for text processing operations
/// </summary>
public class TextProcessingArgs
{
    /// <summary>
    /// The text to process
    /// </summary>
    [JsonPropertyName("text")]
    [JsonRequired]
    [Description("The text content to process")]
    public string Text { get; set; } = string.Empty;

    /// <summary>
    /// The operation to perform
    /// </summary>
    [JsonPropertyName("operation")]
    [JsonRequired]
    [Description("Operation: uppercase, lowercase, reverse, wordcount, trim, base64encode, base64decode")]
    public string Operation { get; set; } = string.Empty;
}
