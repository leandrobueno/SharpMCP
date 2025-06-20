using System.Reflection;
using Microsoft.Extensions.Logging;
using SharpMCP.Core.Server;
using SharpMCP.Core.Tools;
using SharpMCP.Core.Transport;

namespace SharpMCP.Server;

/// <summary>
/// Builder for configuring and creating MCP servers.
/// </summary>
public class McpServerBuilder : IMcpServerBuilder
{
    private readonly McpServerOptions _options = new();
    private readonly List<IMcpTool> _tools = [];
    private readonly List<Assembly> _toolAssemblies = [];
    private IMcpTransport? _transport;
    private ILoggerFactory? _loggerFactory;
    private Type _serverType = typeof(DefaultMcpServer);

    /// <inheritdoc />
    public IMcpServerBuilder WithName(string name)
    {
        _options.Name = name ?? throw new ArgumentNullException(nameof(name));
        return this;
    }

    /// <inheritdoc />
    public IMcpServerBuilder WithVersion(string version)
    {
        _options.Version = version ?? throw new ArgumentNullException(nameof(version));
        return this;
    }

    /// <inheritdoc />
    public IMcpServerBuilder WithServerInfo(string name, string version, string? description = null)
    {
        WithName(name);
        WithVersion(version);
        return this;
    }

    /// <inheritdoc />
    public IMcpServerBuilder WithTransport(IMcpTransport transport)
    {
        _transport = transport ?? throw new ArgumentNullException(nameof(transport));
        return this;
    }

    /// <inheritdoc />
    public IMcpServerBuilder UseStdio()
    {
        // CRITICAL: Ensure stdout is redirected to prevent protocol corruption
        // This must happen before we open the stdout stream
        if (Console.Out != TextWriter.Null)
        {
            Console.SetOut(TextWriter.Null);
            Console.Error.WriteLine("[SharpMCP] WARNING: Console.Out was not redirected. Automatically redirecting to prevent MCP protocol corruption.");
            Console.Error.WriteLine("[SharpMCP] Add 'Console.SetOut(TextWriter.Null)' at the very start of your program to avoid this warning.");
        }

        // Create fresh stdin/stdout streams to ensure no buffered content
        var stdin = Console.OpenStandardInput();
        var stdout = Console.OpenStandardOutput();

        _transport = new Transport.StdioTransport(stdin, stdout, _loggerFactory?.CreateLogger<Transport.StdioTransport>());

        // If no logger factory is set, provide MCP-safe logging by default
        if (_loggerFactory == null)
        {
            WithMcpSafeLogging();
        }

        return this;
    }

    /// <inheritdoc />
    public IMcpServerBuilder WithStdioTransport()
    {
        return UseStdio();
    }

    /// <inheritdoc />
    public IMcpServerBuilder AddTool(IMcpTool tool)
    {
        ArgumentNullException.ThrowIfNull(tool);


        _tools.Add(tool);
        return this;
    }

    /// <inheritdoc />
    public IMcpServerBuilder AddTool<TTool>() where TTool : IMcpTool, new()
    {
        _tools.Add(new TTool());
        return this;
    }

    /// <inheritdoc />
    public IMcpServerBuilder AddToolsFromAssembly(Assembly assembly)
    {
        ArgumentNullException.ThrowIfNull(assembly);


        _toolAssemblies.Add(assembly);
        return this;
    }

    /// <inheritdoc />
    public IMcpServerBuilder AddToolsFromAssembly<TAssemblyMarker>()
    {
        return AddToolsFromAssembly(typeof(TAssemblyMarker).Assembly);
    }

    /// <inheritdoc />
    public IMcpServerBuilder WithLogging(ILoggerFactory loggerFactory)
    {
        return WithLoggerFactory(loggerFactory);
    }

    /// <inheritdoc />
    public IMcpServerBuilder WithLoggerFactory(ILoggerFactory loggerFactory)
    {
        _loggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));

        // Update transport if already created
        if (_transport is Transport.StdioTransport)
        {
            _transport = new Transport.StdioTransport(_loggerFactory.CreateLogger<Transport.StdioTransport>());
        }

        return this;
    }

    /// <summary>
    /// Configures MCP-safe logging that writes to stderr instead of stdout.
    /// This is automatically called when UseStdio() is used without an explicit logger factory.
    /// </summary>
    /// <param name="minimumLevel">The minimum log level. Defaults to Debug for troubleshooting.</param>
    /// <returns>The builder instance.</returns>
    public IMcpServerBuilder WithMcpSafeLogging(LogLevel minimumLevel = LogLevel.Debug)
    {
        var loggerFactory = LoggerFactory.Create(builder =>
        {
            builder.AddMcpConsole(minimumLevel);
        });

        return WithLoggerFactory(loggerFactory);
    }

    /// <inheritdoc />
    public IMcpServerBuilder ConfigureOptions(Action<McpServerOptions> configure)
    {
        ArgumentNullException.ThrowIfNull(configure);


        configure(_options);
        return this;
    }

    /// <summary>
    /// Uses a custom server implementation type.
    /// </summary>
    /// <typeparam name="TServer">The server type to use.</typeparam>
    /// <returns>The builder instance.</returns>
    public IMcpServerBuilder UseServer<TServer>() where TServer : IMcpServer
    {
        _serverType = typeof(TServer);
        return this;
    }

    /// <inheritdoc />
    public IMcpServer Build()
    {
        // Create server instance
        IMcpServer server;
        if (_serverType == typeof(DefaultMcpServer))
        {
            server = new DefaultMcpServer(_options, _loggerFactory?.CreateLogger<DefaultMcpServer>());
        }
        else
        {
            var logger = _loggerFactory?.CreateLogger(_serverType);
            server = (IMcpServer)Activator.CreateInstance(_serverType, _options, logger)!;
        }

        // Register individual tools
        foreach (var tool in _tools)
        {
            server.RegisterTool(tool);
        }

        // Register tools from assemblies
        foreach (var assembly in _toolAssemblies)
        {
            if (server is McpServerBase serverBase)
            {
                serverBase.RegisterToolsFromAssembly(assembly);
            }
        }

        return server;
    }

    /// <inheritdoc />
    public async Task BuildAndRunAsync(CancellationToken cancellationToken = default)
    {
        var server = Build();

        // Set transport if not explicitly set
        if (_transport == null)
        {
            _transport = new Transport.StdioTransport(_loggerFactory?.CreateLogger<Transport.StdioTransport>());
        }

        await server.RunAsync(_transport, cancellationToken).ConfigureAwait(false);
    }
}

/// <summary>
/// Default MCP server implementation.
/// </summary>
internal class DefaultMcpServer : McpServerBase
{
    public DefaultMcpServer(McpServerOptions options, ILogger<DefaultMcpServer>? logger = null)
        : base(options, logger)
    {
    }
}
