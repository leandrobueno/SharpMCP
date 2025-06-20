using System.Collections.Concurrent;
using System.Reflection;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SharpMCP.Core.Protocol;
using SharpMCP.Core.Server;
using SharpMCP.Core.Tools;
using SharpMCP.Core.Transport;

namespace SharpMCP.Server;

/// <summary>
/// Base class for implementing MCP servers.
/// Provides core functionality for handling JSON-RPC messages, managing tools, and server lifecycle.
/// </summary>
public abstract class McpServerBase : IMcpServer
{
    private readonly ConcurrentDictionary<string, IMcpTool> _tools = new();
    private readonly ILogger<McpServerBase>? _logger;
    private readonly McpServerOptions _options;
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        WriteIndented = false,
        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
    };
    private bool _disposed;

    /// <inheritdoc />
    public string Name => _options.Name;

    /// <inheritdoc />
    public string Version => _options.Version;

    /// <inheritdoc />
    public ServerCapabilities Capabilities { get; private set; } = new();

    /// <inheritdoc />
    public IReadOnlyDictionary<string, IMcpTool> Tools => _tools;

    /// <summary>
    /// Occurs when the server starts.
    /// </summary>
    public event EventHandler<McpServerEventArgs>? Started;

    /// <summary>
    /// Occurs when the server stops.
    /// </summary>
    public event EventHandler<McpServerEventArgs>? Stopped;

    /// <summary>
    /// Occurs when a tool is executed.
    /// </summary>
    public event EventHandler<ToolExecutionEventArgs>? ToolExecuted;

    /// <summary>
    /// Initializes a new instance of the <see cref="McpServerBase"/> class.
    /// </summary>
    /// <param name="options">Server configuration options.</param>
    /// <param name="logger">Optional logger.</param>
    protected McpServerBase(IOptions<McpServerOptions> options, ILogger<McpServerBase>? logger = null)
    {
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
        _logger = logger;
        UpdateCapabilities();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="McpServerBase"/> class.
    /// </summary>
    /// <param name="options">Server configuration options.</param>
    /// <param name="logger">Optional logger.</param>
    protected McpServerBase(McpServerOptions options, ILogger<McpServerBase>? logger = null)
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));
        _logger = logger;
        UpdateCapabilities();
    }

    /// <inheritdoc />
    public virtual void RegisterTool(IMcpTool tool)
    {
        ArgumentNullException.ThrowIfNull(tool);


        var name = tool.Name;
        if (!_tools.TryAdd(name, tool))
        {

            throw new InvalidOperationException($"Tool '{name}' is already registered");
        }


        _logger?.LogInformation("Registered tool: {ToolName}", name);
        UpdateCapabilities();
    }

    /// <inheritdoc />
    public virtual bool UnregisterTool(string toolName)
    {
        if (string.IsNullOrEmpty(toolName))
        {

            return false;
        }


        var removed = _tools.TryRemove(toolName, out _);
        if (removed)
        {
            _logger?.LogInformation("Unregistered tool: {ToolName}", toolName);
            UpdateCapabilities();
        }
        return removed;
    }

    /// <inheritdoc />
    public virtual async Task RunAsync(IMcpTransport transport, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(transport);


        _logger?.LogInformation("Starting MCP server: {ServerName} v{ServerVersion}", Name, Version);

        OnStarted();

        try
        {
            await ProcessMessagesAsync(transport, cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            await transport.CloseAsync().ConfigureAwait(false);
            OnStopped();
        }
    }

    /// <summary>
    /// Registers tools from an assembly using reflection.
    /// </summary>
    public virtual void RegisterToolsFromAssembly(Assembly assembly)
    {
        ArgumentNullException.ThrowIfNull(assembly);


        var toolTypes = assembly.GetTypes()
            .Where(t => !t.IsAbstract &&
                       typeof(IMcpTool).IsAssignableFrom(t) &&
                       t.GetCustomAttribute<McpToolAttribute>() != null)
            .ToList();

        foreach (var toolType in toolTypes)
        {
            try
            {
                if (Activator.CreateInstance(toolType) is IMcpTool tool)
                {
                    RegisterTool(tool);
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Failed to create tool instance for type {ToolType}", toolType.Name);
                throw;
            }
        }

        _logger?.LogInformation("Registered {Count} tools from assembly {Assembly}",
            toolTypes.Count, assembly.GetName().Name);
    }

    /// <summary>
    /// Executes a tool by name.
    /// </summary>
    protected virtual async Task<ToolResponse> ExecuteToolAsync(string toolName, JsonElement? arguments = null, CancellationToken cancellationToken = default)
    {
        if (!_tools.TryGetValue(toolName, out var tool))
        {
            throw new McpToolException($"Tool '{toolName}' not found");
        }

        _logger?.LogDebug("Executing tool: {ToolName}", toolName);
        var startTime = DateTimeOffset.UtcNow;

        try
        {
            var response = await tool.ExecuteAsync(arguments, cancellationToken).ConfigureAwait(false);
            var duration = DateTimeOffset.UtcNow - startTime;
            OnToolExecuted(toolName, true, duration);
            return response;
        }
        catch (Exception ex)
        {
            var duration = DateTimeOffset.UtcNow - startTime;
            _logger?.LogError(ex, "Tool execution failed: {ToolName}", toolName);
            OnToolExecuted(toolName, false, duration, ex.Message);
            throw new McpToolException($"Tool '{toolName}' execution failed", ex);
        }
    }

    /// <summary>
    /// Processes incoming messages from the transport.
    /// </summary>
    protected virtual async Task ProcessMessagesAsync(IMcpTransport transport, CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                var message = await transport.ReadMessageAsync(cancellationToken).ConfigureAwait(false);
                if (message == null)
                {
                    _logger?.LogDebug("Received null message, client disconnected");
                    break;
                }

                if (message is JsonRpcRequest request)
                {
                    _logger?.LogDebug("Processing request: {Method} with ID: {Id}", request.Method, request.Id);
                    await HandleRequestAsync(transport, request, cancellationToken).ConfigureAwait(false);
                }
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error processing message");
                // Continue processing
            }
        }

        _logger?.LogDebug("Message processing loop ended");
    }

    /// <summary>
    /// Handles a specific JSON-RPC request.
    /// </summary>
    protected virtual async Task HandleRequestAsync(IMcpTransport transport, JsonRpcRequest request, CancellationToken cancellationToken)
    {
        JsonRpcResponse? response = null;

        try
        {
            response = request.Method switch
            {
                "initialize" => await HandleInitializeAsync(request, cancellationToken).ConfigureAwait(false),
                "tools/list" => await HandleToolsListAsync(request, cancellationToken).ConfigureAwait(false),
                "tools/call" => await HandleToolsCallAsync(request, cancellationToken).ConfigureAwait(false),
                "ping" => HandlePing(request),
                _ => new JsonRpcResponse
                {
                    Id = request.Id,
                    Error = new JsonRpcError
                    {
                        Code = JsonRpcErrorCodes.MethodNotFound,
                        Message = $"Method not found: {request.Method}"
                    }
                }
            };
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error handling request: {Method}", request.Method);
            response = new JsonRpcResponse
            {
                Id = request.Id,
                Error = new JsonRpcError
                {
                    Code = JsonRpcErrorCodes.InternalError,
                    Message = "Internal error",
                    Data = ex.Message
                }
            };
        }

        if (response != null && request.Id != null)
        {
            _logger?.LogDebug("Sending response for request ID: {Id}", request.Id);

            // Debug: Log the response structure
            if (_logger?.IsEnabled(LogLevel.Debug) == true)
            {
                var responseJson = JsonSerializer.Serialize(response, _jsonOptions);
                _logger.LogDebug("Response object: {Response}", responseJson);
            }

            await transport.WriteMessageAsync(response, cancellationToken).ConfigureAwait(false);
            _logger?.LogDebug("Response sent successfully");
        }
        else
        {
            _logger?.LogWarning("No response to send for request: {Method}, ID: {Id}", request.Method, request.Id);
        }
    }

    /// <summary>
    /// Handles the initialize request.
    /// </summary>
    protected virtual Task<JsonRpcResponse> HandleInitializeAsync(JsonRpcRequest request, CancellationToken cancellationToken)
    {
        _logger?.LogDebug("Handling initialize request");

        // Create the response structure according to MCP protocol
        // The client expects: { protocolVersion, capabilities, serverInfo }
        var initializeResult = new
        {
            protocolVersion = McpConstants.ProtocolVersion,
            capabilities = new
            {
                tools = _tools.Any() ? new { } : null,
                resources = _options.EnableResources ? new { } : null,
                prompts = _options.EnablePrompts ? new { } : null
            },
            serverInfo = new
            {
                name = Name,
                version = Version
            }
        };

        _logger?.LogDebug("Sending initialize response with protocol version: {Version}", McpConstants.ProtocolVersion);

        return Task.FromResult(new JsonRpcResponse
        {
            Id = request.Id,
            Result = JsonSerializer.SerializeToElement(initializeResult, _jsonOptions)
        });
    }

    /// <summary>
    /// Handles the tools/list request.
    /// </summary>
    protected virtual Task<JsonRpcResponse> HandleToolsListAsync(JsonRpcRequest request, CancellationToken cancellationToken)
    {
        var tools = _tools.Values.Select(t => new Tool
        {
            Name = t.Name,
            Description = t.Description,
            InputSchema = t.GetInputSchema()
        }).ToArray();

        var response = new ToolsListResponse { Tools = tools };

        return Task.FromResult(new JsonRpcResponse
        {
            Id = request.Id,
            Result = JsonSerializer.SerializeToElement(response)
        });
    }

    /// <summary>
    /// Handles the tools/call request.
    /// </summary>
    protected virtual async Task<JsonRpcResponse> HandleToolsCallAsync(JsonRpcRequest request, CancellationToken cancellationToken)
    {
        if (request.Params == null)
        {
            return new JsonRpcResponse
            {
                Id = request.Id,
                Error = new JsonRpcError
                {
                    Code = JsonRpcErrorCodes.InvalidParams,
                    Message = "Missing parameters"
                }
            };
        }

        try
        {
            var callParams = JsonSerializer.Deserialize<ToolCallParams>(request.Params.Value);
            if (callParams == null || string.IsNullOrEmpty(callParams.Name))
            {
                throw new ArgumentException("Invalid tool call parameters");
            }

            var result = await ExecuteToolAsync(callParams.Name, callParams.Arguments, cancellationToken).ConfigureAwait(false);

            return new JsonRpcResponse
            {
                Id = request.Id,
                Result = JsonSerializer.SerializeToElement(result)
            };
        }
        catch (McpToolException ex)
        {
            return new JsonRpcResponse
            {
                Id = request.Id,
                Error = new JsonRpcError
                {
                    Code = JsonRpcErrorCodes.InvalidRequest,
                    Message = ex.Message,
                    Data = ex.InnerException?.Message
                }
            };
        }
    }

    /// <summary>
    /// Handles the ping request.
    /// </summary>
    protected virtual JsonRpcResponse HandlePing(JsonRpcRequest request)
    {
        return new JsonRpcResponse
        {
            Id = request.Id,
            Result = JsonSerializer.SerializeToElement(new { })
        };
    }

    /// <summary>
    /// Updates server capabilities based on registered features.
    /// </summary>
    private void UpdateCapabilities()
    {
        Capabilities = new ServerCapabilities
        {
            Tools = _options.EnableTools && _tools.Any() ? new ToolsCapability() : null,
            Resources = _options.EnableResources ? new ResourcesCapability() : null,
            Prompts = _options.EnablePrompts ? new PromptsCapability() : null
        };
    }

    /// <summary>
    /// Called when the server has started.
    /// </summary>
    protected virtual void OnStarted()
    {
        Started?.Invoke(this, new McpServerEventArgs(this));
    }

    /// <summary>
    /// Called when the server has stopped.
    /// </summary>
    protected virtual void OnStopped()
    {
        Stopped?.Invoke(this, new McpServerEventArgs(this));
    }

    /// <summary>
    /// Called when a tool has been executed.
    /// </summary>
    protected virtual void OnToolExecuted(string toolName, bool success, TimeSpan duration, string? errorMessage = null)
    {
        ToolExecuted?.Invoke(this, new ToolExecutionEventArgs(this, toolName, success, duration, errorMessage));
    }

    /// <summary>
    /// Releases resources used by the server.
    /// </summary>
    protected virtual void Dispose(bool disposing)
    {
        if (_disposed)
        {
            return;
        }


        if (disposing)
        {
            _tools.Clear();
        }

        _disposed = true;
    }

    /// <inheritdoc />
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}
