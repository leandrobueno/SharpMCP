using SharpMCP;
using SharpMCP.Core.Tools;
using SharpMCP.Core.Protocol;
using SharpMCP.Core.Utilities;
using System.ComponentModel;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace McpToolSetTemplate.Tools;

/// <summary>
/// Tool for data validation operations
/// </summary>
[McpTool("validator", "Validates data against various rules and patterns")]
public class ValidationTool : McpToolBase<ValidationArgs>
{
    protected override Task<ToolResponse> ExecuteAsync(ValidationArgs args, CancellationToken cancellationToken)
    {
        var results = new List<ValidationResult>();

        foreach (var rule in args.Rules)
        {
            var result = ValidateRule(args.Data, rule);
            results.Add(result);
        }

        var response = new
        {
            isValid = results.All(r => r.IsValid),
            results,
            summary = $"{results.Count(r => r.IsValid)}/{results.Count} rules passed"
        };

        return Task.FromResult(ToolResponseBuilder.Success(response));
    }

    private ValidationResult ValidateRule(string data, ValidationRule rule)
    {
        try
        {
            bool isValid = rule.Type switch
            {
                "regex" => Regex.IsMatch(data, rule.Pattern ?? ""),
                "email" => IsValidEmail(data),
                "url" => Uri.TryCreate(data, UriKind.Absolute, out var uri) && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps),
                "json" => IsValidJson(data),
                "numeric" => double.TryParse(data, out _),
                "length" => ValidateLength(data, rule),
                "contains" => data.Contains(rule.Pattern ?? "", StringComparison.OrdinalIgnoreCase),
                "startswith" => data.StartsWith(rule.Pattern ?? "", StringComparison.OrdinalIgnoreCase),
                "endswith" => data.EndsWith(rule.Pattern ?? "", StringComparison.OrdinalIgnoreCase),
                _ => throw new ArgumentException($"Unknown validation type: {rule.Type}")
            };

            return new ValidationResult
            {
                RuleName = rule.Name,
                IsValid = isValid,
                Message = isValid ? "Validation passed" : $"Failed {rule.Type} validation"
            };
        }
        catch (Exception ex)
        {
            return new ValidationResult
            {
                RuleName = rule.Name,
                IsValid = false,
                Message = $"Validation error: {ex.Message}"
            };
        }
    }

    private bool IsValidEmail(string email)
    {
        return Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$");
    }

    private bool IsValidJson(string json)
    {
        try
        {
            System.Text.Json.JsonDocument.Parse(json);
            return true;
        }
        catch
        {
            return false;
        }
    }

    private bool ValidateLength(string data, ValidationRule rule)
    {
        var length = data.Length;
        if (rule.MinLength.HasValue && length < rule.MinLength.Value) return false;
        if (rule.MaxLength.HasValue && length > rule.MaxLength.Value) return false;
        return true;
    }
}

/// <summary>
/// Arguments for validation operations
/// </summary>
public class ValidationArgs
{
    /// <summary>
    /// The data to validate
    /// </summary>
    [JsonPropertyName("data")]
    [JsonRequired]
    [Description("The data to validate")]
    public string Data { get; set; } = string.Empty;

    /// <summary>
    /// Validation rules to apply
    /// </summary>
    [JsonPropertyName("rules")]
    [JsonRequired]
    [Description("Array of validation rules to apply")]
    public ValidationRule[] Rules { get; set; } = Array.Empty<ValidationRule>();
}

/// <summary>
/// Validation rule definition
/// </summary>
public class ValidationRule
{
    /// <summary>
    /// Name of the rule
    /// </summary>
    [JsonPropertyName("name")]
    [JsonRequired]
    [Description("Name of the validation rule")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Type of validation
    /// </summary>
    [JsonPropertyName("type")]
    [JsonRequired]
    [Description("Type: regex, email, url, json, numeric, length, contains, startswith, endswith")]
    public string Type { get; set; } = string.Empty;

    /// <summary>
    /// Pattern for regex/contains validations
    /// </summary>
    [JsonPropertyName("pattern")]
    [Description("Pattern for regex or text matching")]
    public string? Pattern { get; set; }

    /// <summary>
    /// Minimum length for length validation
    /// </summary>
    [JsonPropertyName("minLength")]
    [Description("Minimum length for length validation")]
    public int? MinLength { get; set; }

    /// <summary>
    /// Maximum length for length validation
    /// </summary>
    [JsonPropertyName("maxLength")]
    [Description("Maximum length for length validation")]
    public int? MaxLength { get; set; }
}

/// <summary>
/// Validation result
/// </summary>
public class ValidationResult
{
    [JsonPropertyName("ruleName")]
    public string RuleName { get; set; } = string.Empty;

    [JsonPropertyName("isValid")]
    public bool IsValid { get; set; }

    [JsonPropertyName("message")]
    public string Message { get; set; } = string.Empty;
}
