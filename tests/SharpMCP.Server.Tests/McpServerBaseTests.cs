using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using SharpMCP.Core.Protocol;
using SharpMCP.Core.Server;
using SharpMCP.Core.Transport;
using System.Text.Json;
using Xunit;

namespace SharpMCP.Server.Tests;

/// <summary>
/// Tests for the McpServerBase class functionality.
/// </summary>
public class McpServerBaseTests
{
    private readonly Mock<IMcpTransport> _transportMock;
    private readonly Mock<ILogger<McpServerBase>> _loggerMock;
    private readonly TestMcpServer _server;

    public McpServerBaseTests()
    {
        _transportMock = new Mock<IMcpTransport>();
        _loggerMock = new Mock<ILogger<McpServerBase>>();
        var options = new McpServerOptions
        {
            Name = "TestServer",
            Version = "1.0.0",
            EnableTools = true
        };
        _server = new TestMcpServer(options, _loggerMock.Object);
    }

    [Fact]
    public async Task RunAsync_Should_Process_Messages()
    {
        // Arrange
        var cts = new CancellationTokenSource();
        _transportMock.SetupSequence(t => t.ReadMessageAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync((JsonRpcMessage?)null); // Simulate disconnect

        _transportMock.Setup(t => t.CloseAsync())
            .Returns(Task.CompletedTask);

        // Act
        await _server.RunAsync(_transportMock.Object, cts.Token);

        // Assert
        _transportMock.Verify(t => t.ReadMessageAsync(It.IsAny<CancellationToken>()), Times.AtLeastOnce);
        _transportMock.Verify(t => t.CloseAsync(), Times.Once);
    }

    [Fact]
    public async Task HandleInitializeRequest_Should_Return_ServerInfo()
    {
        // Arrange
        var initializeRequest = new JsonRpcRequest
        {
            JsonRpc = "2.0",
            Id = JsonDocument.Parse("\"1\"").RootElement,
            Method = "initialize",
            Params = JsonDocument.Parse(@"{
                ""protocolVersion"": ""1.0"",
                ""capabilities"": {},
                ""clientInfo"": {
                    ""name"": ""TestClient"",
                    ""version"": ""1.0.0""
                }
            }").RootElement
        };

        JsonRpcResponse? capturedResponse = null;
        _transportMock.Setup(t => t.WriteMessageAsync(It.IsAny<JsonRpcResponse>(), It.IsAny<CancellationToken>()))
            .Callback<JsonRpcMessage, CancellationToken>((msg, _) => capturedResponse = msg as JsonRpcResponse)
            .Returns(Task.CompletedTask);

        // Act
        await _server.TestHandleRequestAsync(_transportMock.Object, initializeRequest);

        // Assert
        capturedResponse.Should().NotBeNull();
        capturedResponse!.Id!.Value.GetString().Should().Be("1");
        capturedResponse.Error.Should().BeNull();
        capturedResponse.Result.Should().NotBeNull();

        var result = capturedResponse.Result!.Value;
        result.GetProperty("protocolVersion").GetString().Should().Be("2024-11-05");
        result.GetProperty("serverInfo").GetProperty("name").GetString().Should().Be("TestServer");
    }

    [Fact]
    public async Task HandleInvalidMethod_Should_Return_MethodNotFound()
    {
        // Arrange
        var request = new JsonRpcRequest
        {
            JsonRpc = "2.0",
            Id = JsonDocument.Parse("\"2\"").RootElement,
            Method = "invalid_method"
        };

        JsonRpcResponse? capturedResponse = null;
        _transportMock.Setup(t => t.WriteMessageAsync(It.IsAny<JsonRpcResponse>(), It.IsAny<CancellationToken>()))
            .Callback<JsonRpcMessage, CancellationToken>((msg, _) => capturedResponse = msg as JsonRpcResponse)
            .Returns(Task.CompletedTask);

        // Act
        await _server.TestHandleRequestAsync(_transportMock.Object, request);

        // Assert
        capturedResponse.Should().NotBeNull();
        capturedResponse!.Error.Should().NotBeNull();
        capturedResponse.Error!.Code.Should().Be(JsonRpcErrorCodes.MethodNotFound);
    }

    [Fact]
    public async Task HandleToolsListRequest_Should_Return_RegisteredTools()
    {
        // Arrange
        _server.RegisterTool(new TestTool());

        var request = new JsonRpcRequest
        {
            JsonRpc = "2.0",
            Id = JsonDocument.Parse("\"3\"").RootElement,
            Method = "tools/list"
        };

        JsonRpcResponse? capturedResponse = null;
        _transportMock.Setup(t => t.WriteMessageAsync(It.IsAny<JsonRpcResponse>(), It.IsAny<CancellationToken>()))
            .Callback<JsonRpcMessage, CancellationToken>((msg, _) => capturedResponse = msg as JsonRpcResponse)
            .Returns(Task.CompletedTask);

        // Act
        await _server.TestHandleRequestAsync(_transportMock.Object, request);

        // Assert
        capturedResponse.Should().NotBeNull();
        capturedResponse!.Error.Should().BeNull();
        capturedResponse.Result.Should().NotBeNull();

        var tools = capturedResponse.Result!.Value.GetProperty("tools");
        tools.GetArrayLength().Should().Be(1);
        tools[0].GetProperty("name").GetString().Should().Be("test_tool");
    }

    private class TestMcpServer : McpServerBase
    {
        public TestMcpServer(McpServerOptions options, ILogger<McpServerBase> logger)
            : base(options, logger)
        {
        }

        public async Task TestHandleRequestAsync(IMcpTransport transport, JsonRpcRequest request)
        {
            await HandleRequestAsync(transport, request, CancellationToken.None);
        }
    }

    private class TestTool : Core.Tools.IMcpTool
    {
        public string Name => "test_tool";
        public string? Description => "A test tool";

        public JsonElement GetInputSchema()
        {
            return JsonDocument.Parse(@"{
                ""type"": ""object"",
                ""properties"": {
                    ""input"": { ""type"": ""string"" }
                }
            }").RootElement;
        }

        public Task<ToolResponse> ExecuteAsync(JsonElement? arguments, CancellationToken cancellationToken = default)
        {
            var input = arguments?.GetProperty("input").GetString() ?? "";
            return Task.FromResult(new ToolResponse
            {
                Content = new List<ContentPart>
                {
                    new ContentPart { Type = "text", Text = $"Processed: {input}" }
                }
            });
        }
    }
}
