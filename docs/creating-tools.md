# Creating Tools

Learn how to build custom MCP tools with SharpMCP.

## Tool Basics

### Simple Tool

```csharp
[McpTool("calculate", Description = "Performs basic calculations")]
public class CalculatorTool : McpToolBase<CalculateArgs>
{
    public override string Name => "calculate";
    public override string? Description => "Performs basic calculations";

    protected override Task<ToolResponse> ExecuteAsync(CalculateArgs args, CancellationToken ct)
    {
        var result = args.Operation switch
        {
            "add" => args.A + args.B,
            "subtract" => args.A - args.B,
            "multiply" => args.A * args.B,
            "divide" => args.B != 0 ? args.A / args.B : throw new ArgumentException("Division by zero"),
            _ => throw new ArgumentException($"Unknown operation: {args.Operation}")
        };

        return Task.FromResult(Success($"Result: {result}"));
    }
}

public class CalculateArgs
{
    [JsonRequired]
    [JsonDescription("First number")]
    public double A { get; set; }

    [JsonRequired]
    [JsonDescription("Second number")]
    public double B { get; set; }

    [JsonRequired]
    [JsonDescription("Operation: add, subtract, multiply, divide")]
    [JsonEnum(["add", "subtract", "multiply", "divide"])]
    public string Operation { get; set; } = "";
}
```

### Tool Without Arguments

```csharp
[McpTool("get_time", Description = "Gets current server time")]
public class TimeTool : McpToolBase
{
    public override string Name => "get_time";
    public override string? Description => "Gets current server time";

    protected override Task<ToolResponse> ExecuteAsync(CancellationToken ct)
    {
        var time = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        return Task.FromResult(Success($"Current time: {time}"));
    }
}
```

## Response Types

### Text Response

```csharp
return Success("Simple text response");
```

### Structured Response

```csharp
var data = new { 
    status = "success", 
    value = 42,
    timestamp = DateTime.UtcNow 
};
return Success(JsonSerializer.Serialize(data));
```

### Error Response

```csharp
return Error("Something went wrong");
```

## Schema Attributes

### Required Fields

```csharp
[JsonRequired]
public string RequiredField { get; set; } = "";
```

### Descriptions

```csharp
[JsonDescription("Detailed field description")]
public string DocumentedField { get; set; } = "";
```

### String Constraints

```csharp
[JsonStringConstraints(MinLength = 1, MaxLength = 100, Pattern = @"^[a-zA-Z]+$")]
public string Name { get; set; } = "";
```

### Number Constraints

```csharp
[JsonNumberConstraints(Minimum = 0, Maximum = 100)]
public int Percentage { get; set; }
```

### Array Constraints

```csharp
[JsonArrayConstraints(MinItems = 1, MaxItems = 10, UniqueItems = true)]
public List<string> Items { get; set; } = [];
```

### Enums

```csharp
[JsonEnum(["low", "medium", "high"])]
public string Priority { get; set; } = "medium";

// Or use actual enums
public Priority Level { get; set; } = Priority.Medium;

public enum Priority { Low, Medium, High }
```

## Advanced Patterns

### Async Operations

```csharp
protected override async Task<ToolResponse> ExecuteAsync(FileArgs args, CancellationToken ct)
{
    var content = await File.ReadAllTextAsync(args.Path, ct);
    var processed = await ProcessContentAsync(content, ct);
    return Success(processed);
}
```

### Progress Reporting

```csharp
protected override async Task<ToolResponse> ExecuteAsync(BatchArgs args, CancellationToken ct)
{
    var results = new List<string>();
    
    for (int i = 0; i < args.Items.Count; i++)
    {
        ct.ThrowIfCancellationRequested();
        
        var result = await ProcessItemAsync(args.Items[i], ct);
        results.Add(result);
        
        // Report progress
        var progress = (i + 1) * 100 / args.Items.Count;
        Logger?.LogInformation("Processing: {Progress}%", progress);
    }
    
    return Success(string.Join("\n", results));
}
```

### Validation

```csharp
protected override string? ValidateArguments(MyArgs args)
{
    if (string.IsNullOrEmpty(args.Name))
        return "Name is required";
        
    if (args.Count < 1 || args.Count > 1000)
        return "Count must be between 1 and 1000";
        
    return null; // Valid
}
```

### Dependency Injection

```csharp
public class DatabaseTool : McpToolBase<QueryArgs>
{
    private readonly IDbConnection _connection;
    private readonly ILogger<DatabaseTool> _logger;

    public DatabaseTool(IDbConnection connection, ILogger<DatabaseTool> logger)
    {
        _connection = connection;
        _logger = logger;
    }

    protected override async Task<ToolResponse> ExecuteAsync(QueryArgs args, CancellationToken ct)
    {
        _logger.LogInformation("Executing query: {Query}", args.Sql);
        
        var results = await _connection.QueryAsync(args.Sql);
        return Success(JsonSerializer.Serialize(results));
    }
}
```

## Error Handling

### Custom Exceptions

```csharp
protected override Task<ToolResponse> ExecuteAsync(MyArgs args, CancellationToken ct)
{
    try
    {
        var result = ProcessData(args.Data);
        return Task.FromResult(Success(result));
    }
    catch (ArgumentException ex)
    {
        return Task.FromResult(Error($"Invalid argument: {ex.Message}"));
    }
    catch (FileNotFoundException ex)
    {
        return Task.FromResult(Error($"File not found: {ex.FileName}"));
    }
    catch (Exception ex)
    {
        Logger?.LogError(ex, "Unexpected error in tool execution");
        return Task.FromResult(Error("An unexpected error occurred"));
    }
}
```

### Tool Exceptions

```csharp
if (args.Value < 0)
    throw new McpToolException("Value must be non-negative");
```

## Testing Tools

### Unit Tests

```csharp
[Test]
public async Task CalculatorTool_Add_ReturnsCorrectResult()
{
    // Arrange
    var tool = new CalculatorTool();
    var args = new CalculateArgs { A = 5, B = 3, Operation = "add" };

    // Act
    var response = await tool.ExecuteAsync(args, CancellationToken.None);

    // Assert
    response.Content.Should().NotBeEmpty();
    response.Content[0].Text.Should().Contain("8");
}
```

### Integration Tests

```csharp
[Test]
public async Task Server_ExecutesTool_ReturnsExpectedResponse()
{
    // Arrange
    var server = new McpServerBuilder()
        .AddTool(new CalculatorTool())
        .Build();

    // Act
    var response = await server.ExecuteToolAsync("calculate", new {
        a = 10,
        b = 5,
        operation = "multiply"
    });

    // Assert
    response.Content[0].Text.Should().Contain("50");
}
```

## Best Practices

- Use descriptive tool and argument names
- Provide comprehensive descriptions
- Validate inputs thoroughly
- Handle errors gracefully
- Use appropriate response types
- Add logging for debugging
- Write unit tests
- Document complex tools
