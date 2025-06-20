using SharpMCP.Core.Protocol;

namespace SharpMCP.Core.Transport;

/// <summary>
/// Defines a transport mechanism for MCP communication.
/// </summary>
public interface IMcpTransport : IDisposable
{
    /// <summary>
    /// Gets whether the transport is connected.
    /// </summary>
    bool IsConnected { get; }

    /// <summary>
    /// Reads the next JSON-RPC message from the transport.
    /// </summary>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>The received message, or null if the transport is closed.</returns>
    Task<JsonRpcMessage?> ReadMessageAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Writes a JSON-RPC message to the transport.
    /// </summary>
    /// <param name="message">The message to send.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    Task WriteMessageAsync(JsonRpcMessage message, CancellationToken cancellationToken = default);

    /// <summary>
    /// Closes the transport connection.
    /// </summary>
    Task CloseAsync();
}

/// <summary>
/// Base class for transport implementations.
/// </summary>
public abstract class McpTransportBase : IMcpTransport
{
    private bool _disposed;

    /// <inheritdoc/>
    public abstract bool IsConnected { get; }

    /// <inheritdoc/>
    public abstract Task<JsonRpcMessage?> ReadMessageAsync(CancellationToken cancellationToken = default);

    /// <inheritdoc/>
    public abstract Task WriteMessageAsync(JsonRpcMessage message, CancellationToken cancellationToken = default);

    /// <inheritdoc/>
    public abstract Task CloseAsync();

    /// <inheritdoc/>
    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Releases unmanaged and optionally managed resources.
    /// </summary>
    /// <param name="disposing">True to release both managed and unmanaged resources.</param>
    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                CloseAsync().GetAwaiter().GetResult();
            }
            _disposed = true;
        }
    }
}

/// <summary>
/// Exception thrown when a transport operation fails.
/// </summary>
public class McpTransportException : Exception
{
    /// <summary>
    /// Gets whether the transport connection is closed.
    /// </summary>
    public bool IsConnectionClosed { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="McpTransportException"/> class.
    /// </summary>
    public McpTransportException() { }

    /// <summary>
    /// Initializes a new instance of the <see cref="McpTransportException"/> class with a message.
    /// </summary>
    public McpTransportException(string message) : base(message) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="McpTransportException"/> class with a message and inner exception.
    /// </summary>
    public McpTransportException(string message, Exception innerException)
        : base(message, innerException) { }
}
