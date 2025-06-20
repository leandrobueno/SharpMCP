using SharpMCP.Core.Protocol;

namespace SharpMCP.Core.Utils;

/// <summary>
/// Builder for creating tool responses.
/// </summary>
public class ToolResponseBuilder
{
    private readonly List<ContentPart> _contentParts = [];
    private bool? _isError;

    /// <summary>
    /// Adds text content to the response.
    /// </summary>
    /// <param name="text">The text to add.</param>
    /// <returns>The builder instance.</returns>
    public ToolResponseBuilder WithContent(string text)
    {
        _contentParts.Add(new ContentPart { Text = text });
        return this;
    }

    /// <summary>
    /// Adds multiple text contents to the response.
    /// </summary>
    /// <param name="texts">The texts to add.</param>
    /// <returns>The builder instance.</returns>
    public ToolResponseBuilder WithContents(params string[] texts)
    {
        foreach (var text in texts)
        {
            WithContent(text);
        }
        return this;
    }

    /// <summary>
    /// Adds formatted text content to the response.
    /// </summary>
    /// <param name="format">The format string.</param>
    /// <param name="args">The format arguments.</param>
    /// <returns>The builder instance.</returns>
    public ToolResponseBuilder WithFormattedContent(string format, params object[] args)
    {
        return WithContent(string.Format(format, args));
    }

    /// <summary>
    /// Adds content with a specific type.
    /// </summary>
    /// <param name="type">The content type.</param>
    /// <param name="text">The content text.</param>
    /// <returns>The builder instance.</returns>
    public ToolResponseBuilder WithTypedContent(string type, string text)
    {
        _contentParts.Add(new ContentPart { Type = type, Text = text });
        return this;
    }

    /// <summary>
    /// Marks the response as an error.
    /// </summary>
    /// <param name="errorMessage">The error message.</param>
    /// <returns>The builder instance.</returns>
    public ToolResponseBuilder WithError(string errorMessage)
    {
        _isError = true;
        return WithContent(errorMessage);
    }

    /// <summary>
    /// Marks the response as an error with exception details.
    /// </summary>
    /// <param name="exception">The exception.</param>
    /// <param name="includeStackTrace">Whether to include the stack trace.</param>
    /// <returns>The builder instance.</returns>
    public ToolResponseBuilder WithError(Exception exception, bool includeStackTrace = false)
    {
        _isError = true;
        var message = includeStackTrace
            ? $"{exception.Message}\n\nStack Trace:\n{exception.StackTrace}"
            : exception.Message;
        return WithContent($"Error: {message}");
    }

    /// <summary>
    /// Adds a warning message to the response.
    /// </summary>
    /// <param name="warning">The warning message.</param>
    /// <returns>The builder instance.</returns>
    public ToolResponseBuilder WithWarning(string warning)
    {
        return WithContent($"Warning: {warning}");
    }

    /// <summary>
    /// Adds a success message to the response.
    /// </summary>
    /// <param name="message">The success message.</param>
    /// <returns>The builder instance.</returns>
    public ToolResponseBuilder WithSuccess(string message)
    {
        _isError = false;
        return WithContent(message);
    }

    /// <summary>
    /// Clears all content from the builder.
    /// </summary>
    /// <returns>The builder instance.</returns>
    public ToolResponseBuilder Clear()
    {
        _contentParts.Clear();
        _isError = null;
        return this;
    }

    /// <summary>
    /// Builds the tool response.
    /// </summary>
    /// <returns>The constructed tool response.</returns>
    public ToolResponse Build()
    {
        return new ToolResponse
        {
            Content = _contentParts.ToList(),
            IsError = _isError
        };
    }

    /// <summary>
    /// Creates a new builder instance.
    /// </summary>
    /// <returns>A new builder.</returns>
    public static ToolResponseBuilder Create() => new();

    /// <summary>
    /// Creates a success response with the specified message.
    /// </summary>
    /// <param name="message">The success message.</param>
    /// <returns>The tool response.</returns>
    public static ToolResponse Success(string message)
    {
        return Create().WithSuccess(message).Build();
    }

    /// <summary>
    /// Creates an error response with the specified message.
    /// </summary>
    /// <param name="errorMessage">The error message.</param>
    /// <returns>The tool response.</returns>
    public static ToolResponse Error(string errorMessage)
    {
        return Create().WithError(errorMessage).Build();
    }

    /// <summary>
    /// Creates an error response from an exception.
    /// </summary>
    /// <param name="exception">The exception.</param>
    /// <param name="includeStackTrace">Whether to include the stack trace.</param>
    /// <returns>The tool response.</returns>
    public static ToolResponse Error(Exception exception, bool includeStackTrace = false)
    {
        return Create().WithError(exception, includeStackTrace).Build();
    }
}
