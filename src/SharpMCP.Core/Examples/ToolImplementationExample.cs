using SharpMCP.Core.Protocol;
using SharpMCP.Core.Schema;
using SharpMCP.Core.Tools;
using SharpMCP.Core.Utils;
using System.Text.Json;

namespace SharpMCP.Core.Examples;

/// <summary>
/// Example showing how to implement a simple tool using SharpMCP.Core interfaces.
/// </summary>
public static class ToolImplementationExample
{
    /// <summary>
    /// Example tool arguments class with schema attributes.
    /// </summary>
    [JsonSchema(Title = "Calculator Arguments", Description = "Arguments for calculator operations")]
    public class CalculatorArgs
    {
        [JsonRequired]
        [JsonDescription("The operation to perform: add, subtract, multiply, or divide")]
        [JsonEnum("add", "subtract", "multiply", "divide")]
        public string Operation { get; set; } = "";

        [JsonRequired]
        [JsonDescription("The first number")]
        [JsonNumberConstraints(Minimum = -1000000, Maximum = 1000000, HasMinimum = true, HasMaximum = true)]
        public double A { get; set; }

        [JsonRequired]
        [JsonDescription("The second number")]
        [JsonNumberConstraints(Minimum = -1000000, Maximum = 1000000, HasMinimum = true, HasMaximum = true)]
        public double B { get; set; }
    }

    /// <summary>
    /// Example tool implementation.
    /// </summary>
    [McpTool("calculator", Description = "Performs basic arithmetic operations")]
    public class CalculatorTool : IMcpTool
    {
        public string Name => "calculator";

        public string? Description => "Performs basic arithmetic operations";

        public JsonElement GetInputSchema()
        {
            // In a real implementation, this would be generated from the CalculatorArgs class
            var schema = new JsonSchema
            {
                Type = "object",
                Title = "Calculator Arguments",
                Description = "Arguments for calculator operations",
                Properties = new Dictionary<string, JsonSchema>
                {
                    ["operation"] = new JsonSchema
                    {
                        Type = "string",
                        Description = "The operation to perform: add, subtract, multiply, or divide",
                        Enum = new object[] { "add", "subtract", "multiply", "divide" }
                    },
                    ["a"] = new JsonSchema
                    {
                        Type = "number",
                        Description = "The first number",
                        Minimum = -1000000,
                        Maximum = 1000000
                    },
                    ["b"] = new JsonSchema
                    {
                        Type = "number",
                        Description = "The second number",
                        Minimum = -1000000,
                        Maximum = 1000000
                    }
                },
                Required = ["operation", "a", "b"],
                AdditionalProperties = false
            };

            // Serialize to JsonElement
            var json = JsonSerializer.Serialize(schema);
            using var doc = JsonDocument.Parse(json);
            return doc.RootElement.Clone();
        }

        public Task<ToolResponse> ExecuteAsync(JsonElement? arguments, CancellationToken cancellationToken = default)
        {
            if (!arguments.HasValue)
            {
                return Task.FromResult(ToolResponseBuilder.Error("Missing arguments"));
            }

            try
            {
                // Parse arguments
                var args = JsonSerializer.Deserialize<CalculatorArgs>(arguments.Value.GetRawText());
                if (args == null)
                {
                    return Task.FromResult(ToolResponseBuilder.Error("Invalid arguments"));
                }

                // Perform calculation
                double result = args.Operation.ToLower() switch
                {
                    "add" => args.A + args.B,
                    "subtract" => args.A - args.B,
                    "multiply" => args.A * args.B,
                    "divide" => args.B != 0 ? args.A / args.B : throw new DivideByZeroException(),
                    _ => throw new ArgumentException($"Unknown operation: {args.Operation}")
                };

                // Build response
                var response = ToolResponseBuilder.Create()
                    .WithFormattedContent("{0} {1} {2} = {3}", args.A, args.Operation, args.B, result)
                    .Build();

                return Task.FromResult(response);
            }
            catch (DivideByZeroException)
            {
                return Task.FromResult(ToolResponseBuilder.Error("Division by zero is not allowed"));
            }
            catch (Exception ex)
            {
                return Task.FromResult(ToolResponseBuilder.Error(ex));
            }
        }
    }

    /// <summary>
    /// Example of using the server builder pattern.
    /// </summary>
    public static void ServerBuilderExample()
    {
        // This is how a server would be built using the interfaces
        // (actual implementation would be in SharpMCP.Server)

        /*
        var server = new McpServerBuilder()
            .WithName("Example Calculator Server")
            .WithVersion("1.0.0")
            .AddTool<CalculatorTool>()
            .EnableTools()
            .UseStdio()
            .Build();

        await server.RunAsync();
        */
    }
}
