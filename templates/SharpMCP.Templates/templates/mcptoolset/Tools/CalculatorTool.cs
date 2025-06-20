using SharpMCP;
using SharpMCP.Core.Tools;
using SharpMCP.Core.Protocol;
using SharpMCP.Core.Utilities;
using System.ComponentModel;
using System.Text.Json.Serialization;

namespace McpToolSetTemplate.Tools;

/// <summary>
/// Calculator tool for mathematical operations
/// </summary>
[McpTool("calculator", "Performs mathematical calculations")]
public class CalculatorTool : McpToolBase<CalculatorArgs>
{
    protected override Task<ToolResponse> ExecuteAsync(CalculatorArgs args, CancellationToken cancellationToken)
    {
        try
        {
            double result = args.Operation switch
            {
                "add" => args.Values.Sum(),
                "subtract" => args.Values.Aggregate((a, b) => a - b),
                "multiply" => args.Values.Aggregate(1.0, (a, b) => a * b),
                "divide" => args.Values.Aggregate((a, b) =>
                {
                    if (b == 0) throw new DivideByZeroException();
                    return a / b;
                }),
                "average" => args.Values.Average(),
                "min" => args.Values.Min(),
                "max" => args.Values.Max(),
                "power" => Math.Pow(args.Values[0], args.Values.ElementAtOrDefault(1)),
                "sqrt" => Math.Sqrt(args.Values[0]),
                _ => throw new ArgumentException($"Unknown operation: {args.Operation}")
            };

            var response = new
            {
                result,
                operation = args.Operation,
                values = args.Values,
                precision = Math.Round(result, args.Precision ?? 2)
            };

            return Task.FromResult(ToolResponseBuilder.Success(response));
        }
        catch (Exception ex)
        {
            return Task.FromResult(ToolResponseBuilder.Error($"Calculation error: {ex.Message}"));
        }
    }
}

/// <summary>
/// Arguments for calculator operations
/// </summary>
public class CalculatorArgs
{
    /// <summary>
    /// The values to operate on
    /// </summary>
    [JsonPropertyName("values")]
    [JsonRequired]
    [Description("Array of numerical values to operate on")]
    public double[] Values { get; set; } = Array.Empty<double>();

    /// <summary>
    /// The mathematical operation to perform
    /// </summary>
    [JsonPropertyName("operation")]
    [JsonRequired]
    [Description("Operation: add, subtract, multiply, divide, average, min, max, power, sqrt")]
    public string Operation { get; set; } = string.Empty;

    /// <summary>
    /// Number of decimal places for the result
    /// </summary>
    [JsonPropertyName("precision")]
    [Description("Number of decimal places for the result")]
    [DefaultValue(2)]
    public int? Precision { get; set; }
}
