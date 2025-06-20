# Configuration

Configure your MCP server and tools for different environments and use cases.

## Server Configuration

### Basic Configuration

```csharp
var server = new McpServerBuilder()
    .WithName("MyServer")
    .WithVersion("1.0.0")
    .WithDescription("Custom MCP server")
    .Build();
```

### Advanced Options

```csharp
var options = new McpServerOptions
{
    Name = "MyServer",
    Version = "1.0.0",
    Description = "Advanced server configuration",
    MaxConcurrentTools = 10,
    RequestTimeout = TimeSpan.FromSeconds(30),
    EnableDetailedErrors = true,
    LogLevel = LogLevel.Information
};

var server = new McpServerBuilder()
    .WithOptions(options)
    .Build();
```

## Environment Variables

### Standard Variables

```bash
# Server identification
MCP_SERVER_NAME="MyServer"
MCP_SERVER_VERSION="1.0.0"
MCP_SERVER_DESCRIPTION="Production server"

# Logging
MCP_LOG_LEVEL="Information"
MCP_ENABLE_DETAILED_ERRORS="false"

# Performance
MCP_MAX_CONCURRENT_TOOLS="5"
MCP_REQUEST_TIMEOUT="30"
```

### Reading in Code

```csharp
var serverName = Environment.GetEnvironmentVariable("MCP_SERVER_NAME") ?? "DefaultServer";
var logLevel = Enum.Parse<LogLevel>(
    Environment.GetEnvironmentVariable("MCP_LOG_LEVEL") ?? "Information"
);

var options = new McpServerOptions
{
    Name = serverName,
    LogLevel = logLevel
};
```

## Configuration Files

### appsettings.json

```json
{
  "McpServer": {
    "Name": "MyServer",
    "Version": "1.0.0",
    "Description": "Configured via JSON",
    "MaxConcurrentTools": 10,
    "RequestTimeoutSeconds": 30,
    "EnableDetailedErrors": false
  },
  "FileSystemTools": {
    "AllowedDirectories": [
      "/safe/path/one",
      "/safe/path/two"
    ],
    "MaxFileSize": 10485760,
    "AllowedExtensions": [".txt", ".json", ".csv"]
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "SharpMCP": "Debug"
    }
  }
}
```

### Configuration Binding

```csharp
public class ServerConfig
{
    public string Name { get; set; } = "";
    public string Version { get; set; } = "";
    public string Description { get; set; } = "";
    public int MaxConcurrentTools { get; set; } = 5;
    public int RequestTimeoutSeconds { get; set; } = 30;
    public bool EnableDetailedErrors { get; set; } = false;
}

public class FileSystemConfig
{
    public List<string> AllowedDirectories { get; set; } = [];
    public long MaxFileSize { get; set; } = 10 * 1024 * 1024; // 10MB
    public List<string> AllowedExtensions { get; set; } = [];
}

// In Program.cs
var configuration = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json")
    .AddEnvironmentVariables()
    .Build();

var serverConfig = configuration.GetSection("McpServer").Get<ServerConfig>();
var fileConfig = configuration.GetSection("FileSystemTools").Get<FileSystemConfig>();

var server = new McpServerBuilder()
    .WithName(serverConfig.Name)
    .WithVersion(serverConfig.Version)
    .AddFileSystemTools(fileConfig.AllowedDirectories)
    .Build();
```

## Tool Configuration

### Configurable Tools

```csharp
public class DatabaseTool : McpToolBase<QueryArgs>
{
    private readonly DatabaseConfig _config;

    public DatabaseTool(DatabaseConfig config)
    {
        _config = config;
    }

    protected override Task<ToolResponse> ExecuteAsync(QueryArgs args, CancellationToken ct)
    {
        if (args.Query.Length > _config.MaxQueryLength)
            return Task.FromResult(Error("Query too long"));

        // Use connection string from config
        using var connection = new SqlConnection(_config.ConnectionString);
        // ... execute query
    }
}

public class DatabaseConfig
{
    public string ConnectionString { get; set; } = "";
    public int MaxQueryLength { get; set; } = 1000;
    public int QueryTimeoutSeconds { get; set; } = 30;
    public List<string> AllowedTables { get; set; } = [];
}

// Registration
var dbConfig = configuration.GetSection("Database").Get<DatabaseConfig>();
server.AddTool(new DatabaseTool(dbConfig));
```

### Factory Pattern

```csharp
public static class ToolFactory
{
    public static List<IMcpTool> CreateTools(IConfiguration configuration)
    {
        var tools = new List<IMcpTool>();

        // File system tools
        var fileConfig = configuration.GetSection("FileSystem").Get<FileSystemConfig>();
        if (fileConfig?.Enabled == true)
        {
            tools.AddRange(CreateFileSystemTools(fileConfig));
        }

        // Database tools
        var dbConfig = configuration.GetSection("Database").Get<DatabaseConfig>();
        if (dbConfig?.Enabled == true)
        {
            tools.Add(new DatabaseTool(dbConfig));
        }

        return tools;
    }
}

// Usage
var tools = ToolFactory.CreateTools(configuration);
var builder = new McpServerBuilder();
foreach (var tool in tools)
{
    builder.AddTool(tool);
}
```

## Logging Configuration

### Microsoft.Extensions.Logging

```csharp
var server = new McpServerBuilder()
    .WithName("MyServer")
    .WithLogging(builder => builder
        .AddConsole()
        .AddFile("logs/server.log")
        .SetMinimumLevel(LogLevel.Information))
    .Build();
```

### Structured Logging

```csharp
public class CustomTool : McpToolBase<MyArgs>
{
    private readonly ILogger<CustomTool> _logger;

    public CustomTool(ILogger<CustomTool> logger)
    {
        _logger = logger;
    }

    protected override async Task<ToolResponse> ExecuteAsync(MyArgs args, CancellationToken ct)
    {
        _logger.LogInformation("Executing tool with {ArgCount} arguments", args.Items.Count);
        
        using var scope = _logger.BeginScope("ToolExecution");
        
        try
        {
            var result = await ProcessAsync(args, ct);
            _logger.LogInformation("Tool completed successfully");
            return Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Tool execution failed");
            return Error("Processing failed");
        }
    }
}
```

## Production Configuration

### Docker

```dockerfile
FROM mcr.microsoft.com/dotnet/runtime:9.0
WORKDIR /app
COPY bin/Release/net9.0/publish/ .

# Environment variables
ENV MCP_SERVER_NAME="ProductionServer"
ENV MCP_LOG_LEVEL="Warning"
ENV MCP_ENABLE_DETAILED_ERRORS="false"

ENTRYPOINT ["dotnet", "MyMcpServer.dll"]
```

### Kubernetes ConfigMap

```yaml
apiVersion: v1
kind: ConfigMap
metadata:
  name: mcp-server-config
data:
  appsettings.json: |
    {
      "McpServer": {
        "Name": "K8sServer",
        "MaxConcurrentTools": 20,
        "EnableDetailedErrors": false
      },
      "Logging": {
        "LogLevel": {
          "Default": "Warning"
        }
      }
    }
```

## Development vs Production

### Development Settings

```json
{
  "McpServer": {
    "EnableDetailedErrors": true,
    "RequestTimeoutSeconds": 300
  },
  "Logging": {
    "LogLevel": {
      "Default": "Debug",
      "SharpMCP": "Trace"
    }
  }
}
```

### Production Settings

```json
{
  "McpServer": {
    "EnableDetailedErrors": false,
    "RequestTimeoutSeconds": 30,
    "MaxConcurrentTools": 5
  },
  "Logging": {
    "LogLevel": {
      "Default": "Warning",
      "Microsoft": "Error"
    }
  }
}
```

## Performance Tuning

### Concurrency Settings

```csharp
var options = new McpServerOptions
{
    MaxConcurrentTools = Environment.ProcessorCount * 2,
    RequestTimeout = TimeSpan.FromSeconds(30),
    
    // Custom thread pool settings
    MinWorkerThreads = 10,
    MaxWorkerThreads = 100
};
```

### Memory Configuration

```csharp
// Configure JSON serialization for performance
services.Configure<JsonOptions>(options =>
{
    options.SerializerOptions.DefaultBufferSize = 4096;
    options.SerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
});

// Configure file processing limits
var fileConfig = new FileSystemConfig
{
    MaxFileSize = 50 * 1024 * 1024, // 50MB
    MaxConcurrentReads = 4,
    BufferSize = 8192
};
```

## Best Practices

- Use configuration files for complex settings
- Environment variables for deployment-specific values
- Validate configuration at startup
- Provide sensible defaults
- Document all configuration options
- Use typed configuration classes
- Separate development and production configs
- Log configuration values (excluding secrets)
- Test with different configurations
