using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using SharpMCP.Core.Protocol;
using SharpMCP.Core.Transport;

namespace SharpMCP.Server.Transport;

/// <summary>
/// Transport implementation that communicates via stdin/stdout.
/// Used for standard MCP server communication with clients.
/// </summary>
public class StdioTransport : McpTransportBase
{
    private readonly Stream _input;
    private readonly Stream _output;
    private readonly StreamReader _reader;
    private readonly StreamWriter _writer;
    private readonly ILogger<StdioTransport>? _logger;
    private readonly SemaphoreSlim _writeLock = new(1, 1);
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        AllowTrailingCommas = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        WriteIndented = false,
        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
    };
    private bool _disposed;
    private bool _isConnected;

    /// <inheritdoc />
    public override bool IsConnected => _isConnected && _input.CanRead && _output.CanWrite;

    /// <summary>
    /// Initializes a new instance of the <see cref="StdioTransport"/> class.
    /// </summary>
    /// <param name="logger">Optional logger for debugging.</param>
    public StdioTransport(ILogger<StdioTransport>? logger = null)
        : this(Console.OpenStandardInput(), Console.OpenStandardOutput(), logger)
    {
        // Ensure stdout is redirected for MCP protocol compliance
        if (Console.Out != TextWriter.Null)
        {
            Console.SetOut(TextWriter.Null);
        }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="StdioTransport"/> class with custom streams.
    /// </summary>
    /// <param name="input">Input stream to read from.</param>
    /// <param name="output">Output stream to write to.</param>
    /// <param name="logger">Optional logger for debugging.</param>
    public StdioTransport(Stream input, Stream output, ILogger<StdioTransport>? logger = null)
    {
        _input = input ?? throw new ArgumentNullException(nameof(input));
        _output = output ?? throw new ArgumentNullException(nameof(output));
        _logger = logger;

        // If using standard streams, ensure stdout is redirected for MCP protocol compliance
        if (_output == Console.OpenStandardOutput())
        {
            Console.SetOut(TextWriter.Null);
        }

        // CRITICAL: Use UTF-8 WITHOUT BOM for MCP JSON-RPC communication
        var utf8NoBom = new UTF8Encoding(false);

        _reader = new StreamReader(_input, utf8NoBom, detectEncodingFromByteOrderMarks: false, bufferSize: 4096, leaveOpen: true);
        _writer = new StreamWriter(_output, utf8NoBom, bufferSize: 1024, leaveOpen: true)
        {
            AutoFlush = true,
            NewLine = "\n"
        };

        _isConnected = true;
        _logger?.LogDebug("StdioTransport initialized");
    }

    /// <inheritdoc />
    public override async Task<JsonRpcMessage?> ReadMessageAsync(CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();

        try
        {
            var line = await _reader.ReadLineAsync(cancellationToken).ConfigureAwait(false);

            if (line == null)
            {
                _logger?.LogDebug("Received null from stdin, connection closed");
                _isConnected = false;
                return null;
            }

            if (string.IsNullOrWhiteSpace(line))
            {
                // Skip empty lines
                return await ReadMessageAsync(cancellationToken).ConfigureAwait(false);
            }

            _logger?.LogDebug("Received message: {Message}", line);

            try
            {
                // Try to deserialize as request first
                if (line.Contains("\"method\""))
                {
                    var request = JsonSerializer.Deserialize<JsonRpcRequest>(line, _jsonOptions);
                    if (request == null)
                    {
                        throw new JsonException("Deserialization returned null for request");
                    }
                    if (string.IsNullOrEmpty(request.Method))
                    {
                        throw new JsonException("Request method is null or empty");
                    }
                    return request;
                }
                else
                {
                    var response = JsonSerializer.Deserialize<JsonRpcResponse>(line, _jsonOptions);
                    if (response == null)
                    {
                        throw new JsonException("Deserialization returned null for response");
                    }
                    return response;
                }
            }
            catch (JsonException ex)
            {
                _logger?.LogError(ex, "Failed to parse JSON message: {Message}", line);
                throw new McpTransportException($"Invalid JSON received: {ex.Message}", ex);
            }
        }
        catch (IOException ex)
        {
            _logger?.LogError(ex, "IO error during receive");
            _isConnected = false;
            throw new McpTransportException("IO error during receive", ex) { IsConnectionClosed = true };
        }
        catch (ObjectDisposedException ex)
        {
            _logger?.LogError(ex, "Transport disposed during receive");
            throw new McpTransportException("Transport has been disposed", ex) { IsConnectionClosed = true };
        }
    }

    /// <inheritdoc />
    public override async Task WriteMessageAsync(JsonRpcMessage message, CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();

        ArgumentNullException.ThrowIfNull(message);

        await _writeLock.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            var json = JsonSerializer.Serialize(message, message.GetType(), _jsonOptions);
            _logger?.LogDebug("Sending JSON-RPC message: {Message}", json);

            // Write the JSON followed by a newline
            await _writer.WriteLineAsync(json).ConfigureAwait(false);

            // Force flush to ensure the message is sent immediately
            await _writer.FlushAsync().ConfigureAwait(false);
            await _output.FlushAsync(cancellationToken).ConfigureAwait(false);

            _logger?.LogDebug("Message sent successfully");
        }
        catch (IOException ex)
        {
            _logger?.LogError(ex, "IO error during send");
            _isConnected = false;
            throw new McpTransportException("IO error during send", ex) { IsConnectionClosed = true };
        }
        finally
        {
            _writeLock.Release();
        }
    }

    /// <inheritdoc />
    public override async Task CloseAsync()
    {
        if (!_isConnected)
        {
            return;
        }


        _logger?.LogInformation("StdioTransport closing");
        _isConnected = false;

        try
        {
            await _writer.FlushAsync().ConfigureAwait(false);
        }
        catch { }

        _logger?.LogInformation("StdioTransport closed");
    }

    /// <inheritdoc />
    protected override void Dispose(bool disposing)
    {
        if (_disposed)
        {
            return;
        }


        if (disposing)
        {
            _logger?.LogDebug("Disposing StdioTransport");

            try
            {
                CloseAsync().GetAwaiter().GetResult();
            }
            catch { }

            _writeLock?.Dispose();
            _reader?.Dispose();
            _writer?.Dispose();
        }

        _disposed = true;
        base.Dispose(disposing);
    }

    private void ThrowIfDisposed()
    {
        ObjectDisposedException.ThrowIf(_disposed, nameof(StdioTransport));
    }
}
