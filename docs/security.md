# Security Best Practices

Secure your MCP server and protect against common vulnerabilities.

## Path Validation

### File System Security

```csharp
// ✅ Good: Use SecurityUtils for path validation
var validPath = SecurityUtils.ValidatePath(userPath, allowedDirectories);

// ❌ Bad: Direct file access without validation
var content = File.ReadAllText(userPath); // Vulnerable to path traversal
```

### Path Traversal Prevention

```csharp
public static class SecurityUtils
{
    public static string ValidatePath(string path, List<string> allowedDirectories)
    {
        // Resolve to absolute path
        var fullPath = Path.GetFullPath(path);
        
        // Check if within allowed directories
        foreach (var allowedDir in allowedDirectories)
        {
            var allowedFullPath = Path.GetFullPath(allowedDir);
            if (fullPath.StartsWith(allowedFullPath, StringComparison.OrdinalIgnoreCase))
            {
                return fullPath;
            }
        }
        
        throw new UnauthorizedAccessException($"Access denied to path: {path}");
    }
}
```

### Restricted Directories

```csharp
var server = new McpServerBuilder()
    .AddFileSystemTools(allowedDirectories: [
        @"/app/data",           // ✅ Application data
        @"/tmp/uploads"         // ✅ Temporary files
        // ❌ Never allow: /, /etc, /bin, C:\Windows
    ])
    .Build();
```

## Input Validation

### Argument Validation

```csharp
protected override string? ValidateArguments(FileArgs args)
{
    // Required fields
    if (string.IsNullOrEmpty(args.Path))
        return "Path is required";
    
    // Path format validation
    if (args.Path.Contains(".."))
        return "Path traversal not allowed";
    
    // File extension validation
    var allowedExtensions = new[] { ".txt", ".json", ".csv" };
    var extension = Path.GetExtension(args.Path).ToLowerInvariant();
    if (!allowedExtensions.Contains(extension))
        return $"File type {extension} not allowed";
    
    // Size limits
    if (args.Content?.Length > 10 * 1024 * 1024) // 10MB
        return "Content too large";
    
    return null;
}
```

### SQL Injection Prevention

```csharp
// ✅ Good: Parameterized queries
protected override async Task<ToolResponse> ExecuteAsync(QueryArgs args, CancellationToken ct)
{
    const string sql = "SELECT * FROM users WHERE name = @name AND age > @age";
    var results = await connection.QueryAsync(sql, new { 
        name = args.Name, 
        age = args.MinAge 
    });
    return Success(JsonSerializer.Serialize(results));
}

// ❌ Bad: String concatenation
var sql = $"SELECT * FROM users WHERE name = '{args.Name}'"; // Vulnerable
```

### Command Injection Prevention

```csharp
// ✅ Good: Use ProcessStartInfo with arguments array
var startInfo = new ProcessStartInfo
{
    FileName = "git",
    Arguments = $"clone {Uri.EscapeDataString(args.Repository)}",
    UseShellExecute = false,
    RedirectStandardOutput = true
};

// ❌ Bad: Shell execution with user input
Process.Start("git", $"clone {args.Repository}"); // Vulnerable
```

## Authentication & Authorization

### Tool-Level Authorization

```csharp
[McpTool("admin_action", Description = "Admin-only operation")]
public class AdminTool : McpToolBase<AdminArgs>
{
    protected override Task<ToolResponse> ExecuteAsync(AdminArgs args, CancellationToken ct)
    {
        // Check authorization
        if (!IsAuthorized(args.AuthToken))
            return Task.FromResult(Error("Unauthorized"));
        
        // Perform admin operation
        return Task.FromResult(Success("Operation completed"));
    }
    
    private bool IsAuthorized(string token)
    {
        // Implement your auth logic
        return ValidateToken(token) && HasAdminRole(token);
    }
}
```

### Role-Based Access

```csharp
public enum UserRole { User, Admin, SuperAdmin }

public class SecureTool : McpToolBase<SecureArgs>
{
    private readonly IAuthService _authService;
    
    protected override async Task<ToolResponse> ExecuteAsync(SecureArgs args, CancellationToken ct)
    {
        var user = await _authService.ValidateTokenAsync(args.Token);
        if (user == null)
            return Error("Invalid token");
        
        // Check permissions
        if (!HasPermission(user.Role, RequiredRole))
            return Error("Insufficient permissions");
        
        return await PerformOperationAsync(args, ct);
    }
    
    protected virtual UserRole RequiredRole => UserRole.User;
}

public class AdminTool : SecureTool
{
    protected override UserRole RequiredRole => UserRole.Admin;
}
```

## Data Sanitization

### Output Sanitization

```csharp
protected override Task<ToolResponse> ExecuteAsync(DisplayArgs args, CancellationToken ct)
{
    var userInput = args.Content;
    
    // Sanitize for display
    var sanitized = HtmlEncoder.Default.Encode(userInput);
    
    // Remove sensitive patterns
    sanitized = Regex.Replace(sanitized, @"\b\d{4}[\s-]?\d{4}[\s-]?\d{4}[\s-]?\d{4}\b", "[REDACTED]");
    
    return Task.FromResult(Success(sanitized));
}
```

### Log Sanitization

```csharp
protected override async Task<ToolResponse> ExecuteAsync(MyArgs args, CancellationToken ct)
{
    // ✅ Good: Sanitized logging
    _logger.LogInformation("Processing file: {FileName}", 
        SanitizeForLog(Path.GetFileName(args.Path)));
    
    // ❌ Bad: Raw user input in logs
    _logger.LogInformation("Processing: {RawInput}", args.UserInput);
    
    // Process...
}

private static string SanitizeForLog(string input)
{
    return Regex.Replace(input, @"[^\w\.-]", "_");
}
```

## Error Handling

### Information Disclosure Prevention

```csharp
protected override Task<ToolResponse> ExecuteAsync(MyArgs args, CancellationToken ct)
{
    try
    {
        var result = ProcessData(args);
        return Task.FromResult(Success(result));
    }
    catch (FileNotFoundException)
    {
        // ✅ Good: Generic error message
        return Task.FromResult(Error("File not found"));
    }
    catch (UnauthorizedAccessException)
    {
        // ✅ Good: Don't reveal path details
        return Task.FromResult(Error("Access denied"));
    }
    catch (Exception ex)
    {
        // ✅ Good: Log detailed error privately
        _logger.LogError(ex, "Unexpected error processing request");
        
        // ✅ Good: Return generic error to client
        return Task.FromResult(Error("An error occurred"));
    }
}
```

### Development vs Production Errors

```csharp
public class McpServerOptions
{
    public bool EnableDetailedErrors { get; set; } = false; // Set true only in dev
}

protected ToolResponse HandleError(Exception ex)
{
    if (_options.EnableDetailedErrors)
    {
        // Development: detailed errors
        return Error($"{ex.GetType().Name}: {ex.Message}\n{ex.StackTrace}");
    }
    else
    {
        // Production: generic errors
        return Error("An error occurred");
    }
}
```

## Resource Limits

### File Size Limits

```csharp
public class FileSystemConfig
{
    public long MaxFileSize { get; set; } = 10 * 1024 * 1024; // 10MB
    public int MaxConcurrentReads { get; set; } = 5;
    public TimeSpan OperationTimeout { get; set; } = TimeSpan.FromMinutes(5);
}

protected override async Task<ToolResponse> ExecuteAsync(ReadFileArgs args, CancellationToken ct)
{
    var fileInfo = new FileInfo(validPath);
    
    if (fileInfo.Length > _config.MaxFileSize)
        return Error($"File too large (max: {_config.MaxFileSize:N0} bytes)");
    
    using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
    cts.CancelAfter(_config.OperationTimeout);
    
    var content = await File.ReadAllTextAsync(validPath, cts.Token);
    return Success(content);
}
```

### Rate Limiting

```csharp
public class RateLimitedTool : McpToolBase<MyArgs>
{
    private readonly Dictionary<string, DateTime> _lastRequests = new();
    private readonly TimeSpan _minimumInterval = TimeSpan.FromSeconds(1);
    
    protected override Task<ToolResponse> ExecuteAsync(MyArgs args, CancellationToken ct)
    {
        var clientId = GetClientId(); // Implement client identification
        
        if (_lastRequests.TryGetValue(clientId, out var lastRequest))
        {
            if (DateTime.UtcNow - lastRequest < _minimumInterval)
                return Task.FromResult(Error("Rate limit exceeded"));
        }
        
        _lastRequests[clientId] = DateTime.UtcNow;
        
        // Process request...
        return ProcessRequestAsync(args, ct);
    }
}
```

## Configuration Security

### Secrets Management

```csharp
// ✅ Good: Use environment variables for secrets
var connectionString = Environment.GetEnvironmentVariable("DATABASE_CONNECTION_STRING");

// ✅ Good: Use Azure Key Vault, HashiCorp Vault, etc.
var secret = await keyVaultClient.GetSecretAsync("database-password");

// ❌ Bad: Hardcoded secrets
var connectionString = "Server=prod;Password=secret123;"; // Never do this
```

### Configuration Validation

```csharp
public static void ValidateConfiguration(McpServerOptions options)
{
    if (string.IsNullOrEmpty(options.Name))
        throw new InvalidOperationException("Server name is required");
    
    if (options.RequestTimeout <= TimeSpan.Zero)
        throw new InvalidOperationException("Request timeout must be positive");
    
    if (options.MaxConcurrentTools <= 0)
        throw new InvalidOperationException("Max concurrent tools must be positive");
    
    // Validate in production
    if (!IsDebugMode && options.EnableDetailedErrors)
        throw new InvalidOperationException("Detailed errors should not be enabled in production");
}
```

## Network Security

### Transport Security

```csharp
// For HTTP transport (future)
public class HttpTransportOptions
{
    public bool RequireHttps { get; set; } = true;
    public string[] AllowedOrigins { get; set; } = [];
    public bool EnableCors { get; set; } = false;
    public TimeSpan RequestTimeout { get; set; } = TimeSpan.FromSeconds(30);
}
```

### Input Size Limits

```csharp
public class StdioTransport : IMcpTransport
{
    private const int MaxMessageSize = 1024 * 1024; // 1MB
    
    public async Task<string> ReceiveMessageAsync(CancellationToken ct)
    {
        var message = await ReadLineAsync(ct);
        
        if (message.Length > MaxMessageSize)
            throw new InvalidOperationException("Message too large");
        
        return message;
    }
}
```

## Security Checklist

### Development
- [ ] Enable detailed errors only in development
- [ ] Use HTTPS for all external communications
- [ ] Validate all inputs
- [ ] Sanitize all outputs
- [ ] Use parameterized queries
- [ ] Implement proper error handling
- [ ] Add logging for security events

### Production
- [ ] Disable detailed errors
- [ ] Configure allowed directories restrictively
- [ ] Set resource limits (file size, timeouts)
- [ ] Implement rate limiting
- [ ] Use secrets management
- [ ] Monitor for security events
- [ ] Regular security updates
- [ ] Penetration testing

### Code Review
- [ ] No hardcoded secrets
- [ ] Input validation present
- [ ] Path traversal prevention
- [ ] SQL injection prevention
- [ ] Command injection prevention
- [ ] Proper error handling
- [ ] Resource limits enforced
