using System.Text.Json;
using SharpMCP.Core.Protocol;
using SharpMCP.Core.Schema;
using SharpMCP.Core.Utils;

namespace SharpMCP.Core.Tools;

/// <summary>
/// Base class for implementing MCP tools with strongly-typed arguments.
/// Provides automatic argument parsing and schema generation.
/// </summary>
/// <typeparam name="TArgs">The type of arguments the tool accepts.</typeparam>
public abstract class McpToolBase<TArgs> : IMcpTool where TArgs : class, new()
{
    /// <inheritdoc />
    public abstract string Name { get; }

    /// <inheritdoc />
    public abstract string? Description { get; }

    /// <inheritdoc />
    public virtual async Task<ToolResponse> ExecuteAsync(JsonElement? arguments, CancellationToken cancellationToken = default)
    {
        TArgs args;

        if (arguments == null || arguments.Value.ValueKind == JsonValueKind.Null)
        {
            // Use default instance if no arguments provided
            args = new TArgs();
        }
        else
        {
            try
            {
                // Deserialize arguments to strongly-typed object
                args = JsonSerializer.Deserialize<TArgs>(arguments.Value) ?? new TArgs();
            }
            catch (JsonException ex)
            {
                throw new McpToolException($"Invalid arguments for tool '{Name}': {ex.Message}", ex);
            }
        }

        // Validate arguments if validation is implemented
        var validationError = ValidateArguments(args);
        if (!string.IsNullOrEmpty(validationError))
        {
            throw new McpToolException($"Argument validation failed: {validationError}");
        }

        // Execute the tool with parsed arguments
        return await ExecuteAsync(args, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public virtual JsonElement GetInputSchema()
    {
        var schema = JsonSchemaGenerator.GenerateSchema<TArgs>();
        return JsonSerializer.SerializeToElement(schema);
    }

    /// <summary>
    /// Executes the tool with strongly-typed arguments.
    /// </summary>
    /// <param name="args">The parsed arguments.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The tool response.</returns>
    protected abstract Task<ToolResponse> ExecuteAsync(TArgs args, CancellationToken cancellationToken);

    /// <summary>
    /// Validates the parsed arguments.
    /// </summary>
    /// <param name="args">The arguments to validate.</param>
    /// <returns>An error message if validation fails; otherwise, null or empty string.</returns>
    protected virtual string? ValidateArguments(TArgs args)
    {
        return null;
    }

    /// <summary>
    /// Creates a successful text response.
    /// </summary>
    protected static ToolResponse Success(string text)
    {
        return new ToolResponseBuilder()
            .WithContent(text)
            .Build();
    }

    /// <summary>
    /// Creates a successful response with custom content.
    /// </summary>
    protected static ToolResponse Success(ContentPart content)
    {
        return new ToolResponseBuilder()
            .WithTypedContent(content.Type, content.Text)
            .Build();
    }

    /// <summary>
    /// Creates an error response.
    /// </summary>
    protected static ToolResponse Error(string error)
    {
        return new ToolResponseBuilder()
            .WithError(error)
            .Build();
    }
}

/// <summary>
/// Base class for implementing MCP tools without arguments.
/// </summary>
public abstract class McpToolBase : IMcpTool
{
    /// <inheritdoc />
    public abstract string Name { get; }

    /// <inheritdoc />
    public abstract string? Description { get; }

    /// <inheritdoc />
    public virtual Task<ToolResponse> ExecuteAsync(JsonElement? arguments, CancellationToken cancellationToken = default)
    {
        return ExecuteAsync(cancellationToken);
    }

    /// <inheritdoc />
    public virtual JsonElement GetInputSchema()
    {
        // Tools without arguments have an empty schema
        var schema = new JsonSchema
        {
            Type = "object",
            Properties = []
        };
        return JsonSerializer.SerializeToElement(schema);
    }

    /// <summary>
    /// Executes the tool without arguments.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The tool response.</returns>
    protected abstract Task<ToolResponse> ExecuteAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Creates a successful text response.
    /// </summary>
    protected static ToolResponse Success(string text)
    {
        return new ToolResponseBuilder()
            .WithContent(text)
            .Build();
    }

    /// <summary>
    /// Creates a successful response with custom content.
    /// </summary>
    protected static ToolResponse Success(ContentPart content)
    {
        return new ToolResponseBuilder()
            .WithTypedContent(content.Type, content.Text)
            .Build();
    }

    /// <summary>
    /// Creates an error response.
    /// </summary>
    protected static ToolResponse Error(string error)
    {
        return new ToolResponseBuilder()
            .WithError(error)
            .Build();
    }
}
