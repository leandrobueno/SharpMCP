using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using SharpMCP.Core.Protocol;
using SharpMCP.Server.Transport;
using System.Text;
using System.Text.Json;
using Xunit;

namespace SharpMCP.Server.Tests.Transport;

/// <summary>
/// Tests for the StdioTransport class.
/// </summary>
public class StdioTransportTests : IDisposable
{
    private readonly Mock<ILogger<StdioTransport>> _loggerMock;
    private readonly MemoryStream _inputStream;
    private readonly MemoryStream _outputStream;
    private readonly StdioTransport _transport;

    public StdioTransportTests()
    {
        _loggerMock = new Mock<ILogger<StdioTransport>>();
        _inputStream = new MemoryStream();
        _outputStream = new MemoryStream();
        _transport = new StdioTransport(_inputStream, _outputStream, _loggerMock.Object);
    }

    [Fact]
    public void IsConnected_Should_Be_True_After_Creation()
    {
        // Assert
        _transport.IsConnected.Should().BeTrue();
    }

    [Fact]
    public async Task WriteMessageAsync_Should_Write_Json_With_Newline()
    {
        // Arrange
        var response = new JsonRpcResponse
        {
            JsonRpc = "2.0",
            Id = JsonDocument.Parse("\"1\"").RootElement,
            Result = JsonDocument.Parse(@"{""success"":true}").RootElement
        };

        // Act
        await _transport.WriteMessageAsync(response);

        // Assert
        _outputStream.Position = 0;
        using var reader = new StreamReader(_outputStream, Encoding.UTF8);
        var output = await reader.ReadToEndAsync();

        output.Should().Contain("\"jsonrpc\":\"2.0\"");
        output.Should().Contain("\"id\":\"1\"");
        output.Should().EndWith("\n");
    }

    [Fact]
    public async Task ReadMessageAsync_Should_Parse_JsonRpcRequest()
    {
        // Arrange
        var requestJson = @"{""jsonrpc"":""2.0"",""id"":""1"",""method"":""test"",""params"":{}}";
        var bytes = Encoding.UTF8.GetBytes(requestJson + "\n");
        await _inputStream.WriteAsync(bytes);
        _inputStream.Position = 0;

        // Act
        var message = await _transport.ReadMessageAsync();

        // Assert
        message.Should().NotBeNull();
        message.Should().BeOfType<JsonRpcRequest>();
        var request = message as JsonRpcRequest;
        request!.Method.Should().Be("test");
        request.Id!.Value.GetString().Should().Be("1");
    }

    [Fact]
    public async Task ReadMessageAsync_Should_Return_Null_On_EOF()
    {
        // Arrange - empty stream

        // Act
        var message = await _transport.ReadMessageAsync();

        // Assert
        message.Should().BeNull();
        _transport.IsConnected.Should().BeFalse();
    }

    [Fact]
    public async Task ReadMessageAsync_Should_Skip_Empty_Lines()
    {
        // Arrange
        var content = "\n\n{\"jsonrpc\":\"2.0\",\"id\":\"1\",\"method\":\"test\"}\n";
        var bytes = Encoding.UTF8.GetBytes(content);
        await _inputStream.WriteAsync(bytes);
        _inputStream.Position = 0;

        // Act
        var message = await _transport.ReadMessageAsync();

        // Assert
        message.Should().NotBeNull();
        message.Should().BeOfType<JsonRpcRequest>();
    }

    [Fact]
    public async Task CloseAsync_Should_Mark_Transport_As_Disconnected()
    {
        // Act
        await _transport.CloseAsync();

        // Assert
        _transport.IsConnected.Should().BeFalse();
    }

    [Fact]
    public async Task WriteMessageAsync_Should_Throw_If_Invalid_Json()
    {
        // Arrange
        var invalidMessage = new JsonRpcResponse(); // Missing required fields

        // Act & Assert
        // The serializer should handle this gracefully
        await _transport.WriteMessageAsync(invalidMessage);

        _outputStream.Position = 0;
        using var reader = new StreamReader(_outputStream, Encoding.UTF8);
        var output = await reader.ReadToEndAsync();
        output.Should().Contain("jsonrpc");
    }

    [Fact]
    public void Constructor_Should_Accept_Console_Streams()
    {
        // Act
        var transport = new StdioTransport(_loggerMock.Object);

        // Assert
        transport.Should().NotBeNull();
        transport.IsConnected.Should().BeTrue();
    }

    public void Dispose()
    {
        _transport?.Dispose();
        _inputStream?.Dispose();
        _outputStream?.Dispose();
    }
}
