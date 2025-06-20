using SharpMCP.Core.Tools;
using SharpMCP.Core.Protocol;
using SharpMCP.Core.Utils;
using System.ComponentModel;
using System.Text.Json.Serialization;
using System.Text;

namespace McpToolSetTemplate.Tools;

/// <summary>
/// Tool for generating various types of reports
/// </summary>
public class ReportGeneratorTool : McpToolBase<ReportGeneratorArgs>
{
    /// <inheritdoc />
    public override string Name => "report_generator";

    /// <inheritdoc />
    public override string? Description => "Generates formatted reports from data";
    protected override async Task<ToolResponse> ExecuteAsync(ReportGeneratorArgs args, CancellationToken cancellationToken)
    {
        await Task.Delay(100, cancellationToken);

        var report = args.Format switch
        {
            "markdown" => GenerateMarkdownReport(args),
            "html" => GenerateHtmlReport(args),
            "text" => GenerateTextReport(args),
            "json" => GenerateJsonReport(args),
            _ => throw new ArgumentException($"Unknown format: {args.Format}")
        };

        return ToolResponseBuilder.Success(report);
    }

    private string GenerateMarkdownReport(ReportGeneratorArgs args)
    {
        var sb = new StringBuilder();
        
        sb.AppendLine($"# {args.Title}");
        sb.AppendLine();
        
        if (!string.IsNullOrEmpty(args.Description))
        {
            sb.AppendLine($"> {args.Description}");
            sb.AppendLine();
        }

        sb.AppendLine($"**Generated:** {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC");
        sb.AppendLine();

        foreach (var section in args.Sections)
        {
            sb.AppendLine($"## {section.Title}");
            sb.AppendLine();
            
            if (section.Items != null && section.Items.Any())
            {
                foreach (var item in section.Items)
                {
                    sb.AppendLine($"- **{item.Key}**: {item.Value}");
                }
            }
            else
            {
                sb.AppendLine(section.Content ?? "No content");
            }
            sb.AppendLine();
        }

        if (!string.IsNullOrEmpty(args.Footer))
        {
            sb.AppendLine("---");
            sb.AppendLine(args.Footer);
        }

        return sb.ToString();
    }

    private string GenerateHtmlReport(ReportGeneratorArgs args)
    {
        var sb = new StringBuilder();
        
        sb.AppendLine("<!DOCTYPE html>");
        sb.AppendLine("<html>");
        sb.AppendLine("<head>");
        sb.AppendLine($"<title>{args.Title}</title>");
        sb.AppendLine("<style>");
        sb.AppendLine("body { font-family: Arial, sans-serif; margin: 40px; }");
        sb.AppendLine("h1 { color: #333; }");
        sb.AppendLine("h2 { color: #666; }");
        sb.AppendLine(".meta { color: #888; font-size: 0.9em; }");
        sb.AppendLine(".section { margin: 20px 0; }");
        sb.AppendLine("</style>");
        sb.AppendLine("</head>");
        sb.AppendLine("<body>");
        
        sb.AppendLine($"<h1>{args.Title}</h1>");
        
        if (!string.IsNullOrEmpty(args.Description))
        {
            sb.AppendLine($"<p><em>{args.Description}</em></p>");
        }

        sb.AppendLine($"<p class='meta'>Generated: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC</p>");

        foreach (var section in args.Sections)
        {
            sb.AppendLine("<div class='section'>");
            sb.AppendLine($"<h2>{section.Title}</h2>");
            
            if (section.Items != null && section.Items.Any())
            {
                sb.AppendLine("<ul>");
                foreach (var item in section.Items)
                {
                    sb.AppendLine($"<li><strong>{item.Key}</strong>: {item.Value}</li>");
                }
                sb.AppendLine("</ul>");
            }
            else
            {
                sb.AppendLine($"<p>{section.Content ?? "No content"}</p>");
            }
            sb.AppendLine("</div>");
        }

        if (!string.IsNullOrEmpty(args.Footer))
        {
            sb.AppendLine("<hr>");
            sb.AppendLine($"<p>{args.Footer}</p>");
        }

        sb.AppendLine("</body>");
        sb.AppendLine("</html>");

        return sb.ToString();
    }

    private string GenerateTextReport(ReportGeneratorArgs args)
    {
        var sb = new StringBuilder();
        var separator = new string('=', args.Title.Length);
        
        sb.AppendLine(args.Title.ToUpper());
        sb.AppendLine(separator);
        sb.AppendLine();
        
        if (!string.IsNullOrEmpty(args.Description))
        {
            sb.AppendLine(args.Description);
            sb.AppendLine();
        }

        sb.AppendLine($"Generated: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC");
        sb.AppendLine();

        foreach (var section in args.Sections)
        {
            sb.AppendLine(section.Title);
            sb.AppendLine(new string('-', section.Title.Length));
            
            if (section.Items != null && section.Items.Any())
            {
                foreach (var item in section.Items)
                {
                    sb.AppendLine($"  {item.Key}: {item.Value}");
                }
            }
            else
            {
                sb.AppendLine(section.Content ?? "No content");
            }
            sb.AppendLine();
        }

        if (!string.IsNullOrEmpty(args.Footer))
        {
            sb.AppendLine(new string('-', 40));
            sb.AppendLine(args.Footer);
        }

        return sb.ToString();
    }

    private string GenerateJsonReport(ReportGeneratorArgs args)
    {
        var report = new
        {
            title = args.Title,
            description = args.Description,
            generated = DateTime.UtcNow,
            sections = args.Sections.Select(s => new
            {
                title = s.Title,
                content = s.Content,
                items = s.Items?.Select(i => new { i.Key, i.Value })
            }),
            footer = args.Footer
        };

        return System.Text.Json.JsonSerializer.Serialize(report, new System.Text.Json.JsonSerializerOptions 
        { 
            WriteIndented = true 
        });
    }
}

/// <summary>
/// Arguments for report generation
/// </summary>
public class ReportGeneratorArgs
{
    /// <summary>
    /// Report title
    /// </summary>
    [JsonPropertyName("title")]
    [JsonRequired]
    [Description("Title of the report")]
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Report description
    /// </summary>
    [JsonPropertyName("description")]
    [Description("Description or summary of the report")]
    public string? Description { get; set; }

    /// <summary>
    /// Report format
    /// </summary>
    [JsonPropertyName("format")]
    [JsonRequired]
    [Description("Output format: markdown, html, text, json")]
    public string Format { get; set; } = "markdown";

    /// <summary>
    /// Report sections
    /// </summary>
    [JsonPropertyName("sections")]
    [JsonRequired]
    [Description("Sections of the report")]
    public ReportSection[] Sections { get; set; } = Array.Empty<ReportSection>();

    /// <summary>
    /// Report footer
    /// </summary>
    [JsonPropertyName("footer")]
    [Description("Footer text for the report")]
    public string? Footer { get; set; }
}

/// <summary>
/// Report section
/// </summary>
public class ReportSection
{
    /// <summary>
    /// Section title
    /// </summary>
    [JsonPropertyName("title")]
    [JsonRequired]
    [Description("Title of the section")]
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Section content (for text sections)
    /// </summary>
    [JsonPropertyName("content")]
    [Description("Text content of the section")]
    public string? Content { get; set; }

    /// <summary>
    /// Section items (for key-value sections)
    /// </summary>
    [JsonPropertyName("items")]
    [Description("Key-value items in the section")]
    public ReportItem[]? Items { get; set; }
}

/// <summary>
/// Report item
/// </summary>
public class ReportItem
{
    /// <summary>
    /// Item key
    /// </summary>
    [JsonPropertyName("key")]
    [JsonRequired]
    [Description("Key or label")]
    public string Key { get; set; } = string.Empty;

    /// <summary>
    /// Item value
    /// </summary>
    [JsonPropertyName("value")]
    [JsonRequired]
    [Description("Value or content")]
    public string Value { get; set; } = string.Empty;
}
