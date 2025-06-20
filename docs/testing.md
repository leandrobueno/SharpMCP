# Testing

Test your MCP servers and tools to ensure reliability and correctness.

## Unit Testing Tools

### Basic Tool Testing

```csharp
[Test]
public async Task GreetTool_WithValidArgs_ReturnsGreeting()
{
    // Arrange
    var tool = new GreetTool();
    var args = new GreetArgs { Name = "Alice", Message = "Hello!" };

    // Act
    var response = await tool.ExecuteAsync(args, CancellationToken.None);

    // Assert
    response.Should().NotBeNull();
    response.IsError.Should().BeFalse();
    response.Content.Should().HaveCount(1);
    response.Content[0].Text.Should().Contain("Alice");
    response.Content[0].Text.Should().Contain("Hello!");
}
```

### Error Scenarios

```csharp
[Test]
public async Task CalculatorTool_DivideByZero_ReturnsError()
{
    // Arrange
    var tool = new CalculatorTool();
    var args = new CalculateArgs { A = 10, B = 0, Operation = "divide" };

    // Act
    var response = await tool.ExecuteAsync(args, CancellationToken.None);

    // Assert
    response.IsError.Should().BeTrue();
    response.Content[0].Text.Should().Contain("Division by zero");
}
```

### Validation Testing

```csharp
[Test]
public async Task FileTool_InvalidPath_ThrowsException()
{
    // Arrange
    var tool = new ReadFileTool(["/allowed/path"]);
    var args = new ReadFileArgs { Path = "/forbidden/path/file.txt" };

    // Act & Assert
    var exception = await Assert.ThrowsAsync<McpToolException>(
        () => tool.ExecuteAsync(args, CancellationToken.None)
    );
    exception.Message.Should().Contain("Access denied");
}
```

## Integration Testing

### Server Testing

```csharp
[Test]
public async Task Server_ExecutesTool_ReturnsExpectedResponse()
{
    // Arrange
    var server = new McpServerBuilder()
        .WithName("TestServer")
        .AddTool(new GreetTool())
        .Build();

    var request = new JsonRpcRequest
    {
        Id = "test-1",
        Method = "tools/call",
        Params = JsonSerializer.SerializeToElement(new
        {
            name = "greet",
            arguments = new { name = "Bob", message = "Hi there!" }
        })
    };

    // Act
    var response = await server.HandleRequestAsync(request, CancellationToken.None);

    // Assert
    response.Should().NotBeNull();
    response.Error.Should().BeNull();
    
    var result = response.Result.Deserialize<ToolResponse>();
    result.Content[0].Text.Should().Contain("Bob");
}
```

### Transport Testing

```csharp
[Test]
public async Task StdioTransport_SendReceive_WorksCorrectly()
{
    // Arrange
    var mockInput = new StringReader("""{"jsonrpc":"2.0","id":"1","method":"ping"}""");
    var mockOutput = new StringWriter();
    
    var transport = new StdioTransport(mockInput, mockOutput);
    var server = new McpServerBuilder()
        .WithTransport(transport)
        .Build();

    // Act
    await server.StartAsync();
    await server.SendResponseAsync(new JsonRpcResponse 
    { 
        Id = "1", 
        Result = JsonSerializer.SerializeToElement(new { status = "pong" })
    });

    // Assert
    var output = mockOutput.ToString();
    output.Should().Contain("pong");
}
```

## Test Helpers

### Mock Tools

```csharp
public class MockTool : McpToolBase<MockToolArgs>
{
    public bool WasCalled { get; private set; }
    public MockToolArgs LastArgs { get; private set; }
    public ToolResponse ResponseToReturn { get; set; } = Success("Mock response");

    public override string Name => "mock_tool";
    public override string? Description => "Mock tool for testing";

    protected override Task<ToolResponse> ExecuteAsync(MockToolArgs args, CancellationToken ct)
    {
        WasCalled = true;
        LastArgs = args;
        return Task.FromResult(ResponseToReturn);
    }
}

public class MockToolArgs
{
    public string Input { get; set; } = "";
}
```

### Test Server Builder

```csharp
public static class TestServerBuilder
{
    public static McpServerBuilder CreateTestServer()
    {
        return new McpServerBuilder()
            .WithName("TestServer")
            .WithVersion("1.0.0-test")
            .WithOptions(new McpServerOptions
            {
                EnableDetailedErrors = true,
                RequestTimeout = TimeSpan.FromSeconds(5)
            });
    }

    public static McpServerBuilder WithMockFileSystem(this McpServerBuilder builder)
    {
        var mockFileSystem = new MockFileSystemTools();
        return builder.AddTool(mockFileSystem);
    }
}

// Usage
var server = TestServerBuilder
    .CreateTestServer()
    .WithMockFileSystem()
    .AddTool(new MyCustomTool())
    .Build();
```

### Test Data Builders

```csharp
public class GreetArgsBuilder
{
    private string _name = "DefaultName";
    private string _message = "Default message";

    public GreetArgsBuilder WithName(string name)
    {
        _name = name;
        return this;
    }

    public GreetArgsBuilder WithMessage(string message)
    {
        _message = message;
        return this;
    }

    public GreetArgs Build() => new() { Name = _name, Message = _message };
}

// Usage
var args = new GreetArgsBuilder()
    .WithName("Alice")
    .WithMessage("Hello!")
    .Build();
```

## Async Testing

### Cancellation Testing

```csharp
[Test]
public async Task LongRunningTool_Cancellation_StopsExecution()
{
    // Arrange
    var tool = new LongRunningTool();
    var args = new LongRunningArgs { DurationSeconds = 60 };
    
    using var cts = new CancellationTokenSource();
    cts.CancelAfter(TimeSpan.FromMilliseconds(100));

    // Act & Assert
    await Assert.ThrowsAsync<OperationCanceledException>(
        () => tool.ExecuteAsync(args, cts.Token)
    );
}
```

### Timeout Testing

```csharp
[Test]
public async Task SlowTool_Timeout_ReturnsError()
{
    // Arrange
    var server = new McpServerBuilder()
        .WithOptions(new McpServerOptions 
        { 
            RequestTimeout = TimeSpan.FromMilliseconds(100) 
        })
        .AddTool(new SlowTool())
        .Build();

    // Act
    var task = server.ExecuteToolAsync("slow_tool", new { delay = 1000 });
    
    // Assert
    await Assert.ThrowsAsync<TimeoutException>(() => task);
}
```

## File System Testing

### Temporary Files

```csharp
[Test]
public async Task ReadFileTool_ValidFile_ReturnsContent()
{
    // Arrange
    var tempFile = Path.GetTempFileName();
    var expectedContent = "Test file content";
    await File.WriteAllTextAsync(tempFile, expectedContent);

    try
    {
        var tool = new ReadFileTool([Path.GetTempPath()]);
        var args = new ReadFileArgs { Path = tempFile };

        // Act
        var response = await tool.ExecuteAsync(args, CancellationToken.None);

        // Assert
        response.Content[0].Text.Should().Be(expectedContent);
    }
    finally
    {
        File.Delete(tempFile);
    }
}
```

### Mock File System

```csharp
public class MockFileSystemTool : McpToolBase<ReadFileArgs>
{
    private readonly Dictionary<string, string> _files = new();

    public void AddFile(string path, string content)
    {
        _files[path] = content;
    }

    protected override Task<ToolResponse> ExecuteAsync(ReadFileArgs args, CancellationToken ct)
    {
        if (_files.TryGetValue(args.Path, out var content))
            return Task.FromResult(Success(content));
        
        return Task.FromResult(Error("File not found"));
    }
}

[Test]
public async Task MockFileSystem_FileExists_ReturnsContent()
{
    // Arrange
    var tool = new MockFileSystemTool();
    tool.AddFile("/test/file.txt", "mock content");
    
    var args = new ReadFileArgs { Path = "/test/file.txt" };

    // Act
    var response = await tool.ExecuteAsync(args, CancellationToken.None);

    // Assert
    response.Content[0].Text.Should().Be("mock content");
}
```

## Performance Testing

### Benchmark Testing

```csharp
[Test]
public async Task CalculatorTool_Performance_CompletesQuickly()
{
    // Arrange
    var tool = new CalculatorTool();
    var args = new CalculateArgs { A = 1000, B = 2000, Operation = "add" };
    var stopwatch = Stopwatch.StartNew();

    // Act
    var response = await tool.ExecuteAsync(args, CancellationToken.None);

    // Assert
    stopwatch.Stop();
    response.IsError.Should().BeFalse();
    stopwatch.ElapsedMilliseconds.Should().BeLessThan(100);
}
```

### Concurrent Execution

```csharp
[Test]
public async Task Server_ConcurrentRequests_HandlesCorrectly()
{
    // Arrange
    var server = TestServerBuilder
        .CreateTestServer()
        .AddTool(new CalculatorTool())
        .Build();

    var tasks = Enumerable.Range(1, 10)
        .Select(i => server.ExecuteToolAsync("calculate", new {
            a = i,
            b = i + 1,
            operation = "add"
        }))
        .ToArray();

    // Act
    var responses = await Task.WhenAll(tasks);

    // Assert
    responses.Should().HaveCount(10);
    responses.Should().AllSatisfy(r => r.IsError.Should().BeFalse());
}
```

## Test Organization

### Base Test Class

```csharp
public abstract class ToolTestBase<TTool, TArgs> 
    where TTool : McpToolBase<TArgs>, new()
    where TArgs : class, new()
{
    protected TTool Tool { get; private set; }
    protected ILogger<TTool> Logger { get; private set; }

    [SetUp]
    public virtual void SetUp()
    {
        Logger = Substitute.For<ILogger<TTool>>();
        Tool = CreateTool();
    }

    protected virtual TTool CreateTool() => new();

    protected async Task<ToolResponse> ExecuteAsync(TArgs args)
    {
        return await Tool.ExecuteAsync(args, CancellationToken.None);
    }

    protected async Task<ToolResponse> ExecuteAsync(object args)
    {
        var json = JsonSerializer.Serialize(args);
        var typedArgs = JsonSerializer.Deserialize<TArgs>(json);
        return await ExecuteAsync(typedArgs);
    }
}

// Usage
public class CalculatorToolTests : ToolTestBase<CalculatorTool, CalculateArgs>
{
    [Test]
    public async Task Add_ReturnsSum()
    {
        var response = await ExecuteAsync(new { a = 5, b = 3, operation = "add" });
        response.Content[0].Text.Should().Contain("8");
    }
}
```

### Test Categories

```csharp
[Category("Unit")]
public class CalculatorUnitTests { }

[Category("Integration")]
public class ServerIntegrationTests { }

[Category("Performance")]
public class PerformanceTests { }

// Run specific categories
// dotnet test --filter Category=Unit
```

## Test Configuration

### Test Settings

```json
{
  "Testing": {
    "EnableMockFileSystem": true,
    "TestDataPath": "./TestData",
    "MockDelayMs": 0,
    "EnableDetailedAssertions": true
  }
}
```

### Test Fixtures

```csharp
[TestFixture]
public class FileSystemToolTests
{
    private string _testDirectory;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        _testDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(_testDirectory);
        
        // Create test files
        File.WriteAllText(Path.Combine(_testDirectory, "test.txt"), "test content");
    }

    [OneTimeTearDown]
    public void OneTimeTearDown()
    {
        if (Directory.Exists(_testDirectory))
            Directory.Delete(_testDirectory, true);
    }
}
```

## Best Practices

- Test both success and error scenarios
- Use meaningful test names
- Arrange-Act-Assert pattern
- Mock external dependencies
- Test edge cases and boundary conditions
- Use temporary files/directories for file system tests
- Test cancellation and timeouts
- Include performance tests for critical paths
- Organize tests with categories
- Clean up resources in teardown methods
