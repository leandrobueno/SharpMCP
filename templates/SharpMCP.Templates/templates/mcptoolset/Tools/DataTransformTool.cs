using SharpMCP;
using SharpMCP.Core.Tools;
using SharpMCP.Core.Protocol;
using SharpMCP.Core.Utilities;
using System.ComponentModel;
using System.Text.Json.Serialization;
using System.Text.Json;

namespace McpToolSetTemplate.Tools;

/// <summary>
/// Tool for data transformation operations
/// </summary>
[McpTool("data_transform", "Transforms data between different formats")]
public class DataTransformTool : McpToolBase<DataTransformArgs>
{
    protected override async Task<ToolResponse> ExecuteAsync(DataTransformArgs args, CancellationToken cancellationToken)
    {
        await Task.Delay(50, cancellationToken);

        try
        {
            var result = args.Operation switch
            {
                "json_to_csv" => JsonToCsv(args.Data),
                "csv_to_json" => CsvToJson(args.Data),
                "json_pretty" => PrettyPrintJson(args.Data),
                "json_minify" => MinifyJson(args.Data),
                "extract_keys" => ExtractJsonKeys(args.Data),
                "filter_json" => FilterJson(args.Data, args.FilterPath),
                _ => throw new ArgumentException($"Unknown operation: {args.Operation}")
            };

            return ToolResponseBuilder.Success(result);
        }
        catch (Exception ex)
        {
            return ToolResponseBuilder.Error($"Transform error: {ex.Message}");
        }
    }

    private string JsonToCsv(string jsonData)
    {
        var array = JsonSerializer.Deserialize<JsonElement[]>(jsonData);
        if (array == null || array.Length == 0) return "";

        var headers = array[0].EnumerateObject().Select(p => p.Name).ToList();
        var csv = new List<string> { string.Join(",", headers) };

        foreach (var item in array)
        {
            var values = headers.Select(h =>
            {
                if (item.TryGetProperty(h, out var prop))
                    return prop.ValueKind == JsonValueKind.String ? $"\"{prop.GetString()}\"" : prop.ToString();
                return "";
            });
            csv.Add(string.Join(",", values));
        }

        return string.Join(Environment.NewLine, csv);
    }

    private string CsvToJson(string csvData)
    {
        var lines = csvData.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        if (lines.Length < 2) return "[]";

        var headers = lines[0].Split(',').Select(h => h.Trim('"')).ToArray();
        var result = new List<Dictionary<string, string>>();

        for (int i = 1; i < lines.Length; i++)
        {
            var values = lines[i].Split(',').Select(v => v.Trim('"')).ToArray();
            var row = new Dictionary<string, string>();
            
            for (int j = 0; j < Math.Min(headers.Length, values.Length); j++)
            {
                row[headers[j]] = values[j];
            }
            
            result.Add(row);
        }

        return JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true });
    }

    private string PrettyPrintJson(string json)
    {
        var element = JsonSerializer.Deserialize<JsonElement>(json);
        return JsonSerializer.Serialize(element, new JsonSerializerOptions { WriteIndented = true });
    }

    private string MinifyJson(string json)
    {
        var element = JsonSerializer.Deserialize<JsonElement>(json);
        return JsonSerializer.Serialize(element);
    }

    private string ExtractJsonKeys(string json)
    {
        var element = JsonSerializer.Deserialize<JsonElement>(json);
        var keys = new HashSet<string>();
        ExtractKeysRecursive(element, "", keys);
        return JsonSerializer.Serialize(keys.OrderBy(k => k));
    }

    private void ExtractKeysRecursive(JsonElement element, string prefix, HashSet<string> keys)
    {
        if (element.ValueKind == JsonValueKind.Object)
        {
            foreach (var prop in element.EnumerateObject())
            {
                var key = string.IsNullOrEmpty(prefix) ? prop.Name : $"{prefix}.{prop.Name}";
                keys.Add(key);
                ExtractKeysRecursive(prop.Value, key, keys);
            }
        }
        else if (element.ValueKind == JsonValueKind.Array)
        {
            var index = 0;
            foreach (var item in element.EnumerateArray())
            {
                ExtractKeysRecursive(item, $"{prefix}[{index}]", keys);
                index++;
            }
        }
    }

    private string FilterJson(string json, string? path)
    {
        if (string.IsNullOrEmpty(path)) return json;

        var element = JsonSerializer.Deserialize<JsonElement>(json);
        var pathParts = path.Split('.');
        
        foreach (var part in pathParts)
        {
            if (element.ValueKind == JsonValueKind.Object && element.TryGetProperty(part, out var prop))
            {
                element = prop;
            }
            else
            {
                throw new ArgumentException($"Path '{path}' not found in JSON");
            }
        }

        return JsonSerializer.Serialize(element, new JsonSerializerOptions { WriteIndented = true });
    }
}

/// <summary>
/// Arguments for data transformation
/// </summary>
public class DataTransformArgs
{
    /// <summary>
    /// The data to transform
    /// </summary>
    [JsonPropertyName("data")]
    [JsonRequired]
    [Description("The data to transform")]
    public string Data { get; set; } = string.Empty;

    /// <summary>
    /// The transformation operation
    /// </summary>
    [JsonPropertyName("operation")]
    [JsonRequired]
    [Description("Operation: json_to_csv, csv_to_json, json_pretty, json_minify, extract_keys, filter_json")]
    public string Operation { get; set; } = string.Empty;

    /// <summary>
    /// Path for filtering (used with filter_json)
    /// </summary>
    [JsonPropertyName("filterPath")]
    [Description("JSON path for filtering (e.g., 'data.items.name')")]
    public string? FilterPath { get; set; }
}
