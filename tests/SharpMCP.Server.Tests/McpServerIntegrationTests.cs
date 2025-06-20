using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using SharpMCP.Core.Protocol;
using SharpMCP.Core.Server;
using SharpMCP.Core.Tools;
using SharpMCP.Server.Transport;
using System.Text;
using System.Text.Json;
using Xunit;

namespace SharpMCP.Server.Tests;

/// <summary>
/// Integration tests for end-to-end MCP server functionality.
/// </summary>
public class McpServerIntegrationTests : IDisposable
{
    private readonly MemoryStream _inputStream;
    private readonly MemoryStream _outputStream;
    private readonly Mock<ILogger<McpServerBase>> _serverLoggerMock;
    private readonly Mock<ILogger<StdioTransport>> _transportLoggerMock;
    private readonly TestServer _server;
    private readonly StdioTransport _transport;

    public McpServerIntegrationTests()
    {
        _inputStream = new MemoryStream();
        _outputStream = new MemoryStream();
        _serverLoggerMock = new Mock<ILogger<McpServerBase>>();
        _transportLoggerMock = new Mock<ILogger<StdioTransport>>();

        _transport = new StdioTransport(_inputStream, _outputStream, _transportLoggerMock.Object);
        var options = new McpServerOptions
        {
            Name = "TestServer",
            Version = "1.0.0",
            EnableTools = true
        };
        _server = new TestServer(options, _serverLoggerMock.Object);
    }

    [Fact]
    public async Task Full_Lifecycle_Should_Work()
    {
        // Prepare all messages upfront
        await SendMessageAsync(new JsonRpcRequest
        {
            JsonRpc = "2.0",
            Id = JsonDocument.Parse("\"init-1\"").RootElement,
            Method = "initialize",
            Params = JsonDocument.Parse(@"{
                ""protocolVersion"": ""1.0"",
                ""capabilities"": {},
                ""clientInfo"": {
                    ""name"": ""TestClient"",
                    ""version"": ""1.0.0""
                }
            }").RootElement
        });

        await SendMessageAsync(new JsonRpcRequest
        {
            JsonRpc = "2.0",
            Id = JsonDocument.Parse("\"tools-1\"").RootElement,
            Method = "tools/list"
        });

        await SendMessageAsync(new JsonRpcRequest
        {
            JsonRpc = "2.0",
            Id = JsonDocument.Parse("\"call-1\"").RootElement,
            Method = "tools/call",
            Params = JsonDocument.Parse(@"{
                ""name"": ""echo"",
                ""arguments"": {
                    ""message"": ""Hello, MCP!""
                }
            }").RootElement
        });

        // Reset stream position for reading
        _inputStream.Position = 0;

        // Run server in background task
        var serverTask = Task.Run(async () =>
        {
            var cts = new CancellationTokenSource();
            cts.CancelAfter(TimeSpan.FromSeconds(5)); // Safety timeout
            await _server.RunAsync(_transport, cts.Token);
        });

        // Give server time to process messages
        await Task.Delay(500);

        // Read responses
        var responses = ReadAllResponses();

        // Verify we got responses
        responses.Count.Should().BeGreaterThanOrEqualTo(3);

        // Cancel server
        _transport.Dispose();
        await serverTask;
    }

    private async Task SendMessageAsync(JsonRpcRequest request)
    {
        var json = JsonSerializer.Serialize(request);
        var bytes = Encoding.UTF8.GetBytes(json + "\n");

        await _inputStream.WriteAsync(bytes);
        await _inputStream.FlushAsync();
    }

    private List<JsonRpcResponse> ReadAllResponses()
    {
        var responses = new List<JsonRpcResponse>();
        _outputStream.Position = 0;

        using var reader = new StreamReader(_outputStream, Encoding.UTF8);
        string? line;
        while ((line = reader.ReadLine()) != null)
        {
            if (string.IsNullOrWhiteSpace(line))
            {
                continue;
            }

            try
            {
                var response = JsonSerializer.Deserialize<JsonRpcResponse>(line);
                if (response != null)
                {
                    responses.Add(response);
                }
            }
            catch
            {
                // Skip invalid lines
            }
        }

        return responses;
    }

    public void Dispose()
    {
        _transport?.Dispose();
        _inputStream?.Dispose();
        _outputStream?.Dispose();
    }

    private class TestServer : McpServerBase
    {
        public TestServer(McpServerOptions options, ILogger<McpServerBase> logger)
            : base(options, logger)
        {
            RegisterTool(new EchoTool());
        }

        private class EchoTool : IMcpTool
        {
            public string Name => "echo";
            public string? Description => "Echoes back the provided message";

            public JsonElement GetInputSchema()
            {
                return JsonDocument.Parse(@"{
                    ""type"": ""object"",
                    ""properties"": {
                        ""message"": { ""type"": ""string"", ""description"": ""Message to echo"" }
                    },
                    ""required"": [""message""]
                }").RootElement;
            }

            public Task<ToolResponse> ExecuteAsync(JsonElement? arguments, CancellationToken cancellationToken = default)
            {
                var message = arguments?.GetProperty("message").GetString() ?? "";
                return Task.FromResult(new ToolResponse
                {
                    Content = new List<ContentPart>
                    {
                        new ContentPart { Type = "text", Text = $"Echo: {message}" }
                    }
                });
            }
        }
    }
}
