using SharpMCP.Core.Protocol;
using System.Text.Json;
using Xunit;
using FluentAssertions;

namespace SharpMCP.Core.Tests.Protocol;

public class JsonRpcMessageTests
{
    [Fact]
    public void JsonRpcMessage_ShouldHaveCorrectVersion()
    {
        var request = new JsonRpcRequest { Method = "test" };
        request.JsonRpc.Should().Be("2.0");
    }

    [Fact]
    public void JsonRpcRequest_ShouldSerializeCorrectly()
    {
        var request = new JsonRpcRequest
        {
            Id = JsonSerializer.SerializeToElement(1),
            Method = "test",
            Params = JsonSerializer.SerializeToElement(new { value = "hello" })
        };

        var json = JsonSerializer.Serialize(request);
        json.Should().Contain("\"jsonrpc\":\"2.0\"");
        json.Should().Contain("\"method\":\"test\"");
        json.Should().Contain("\"id\":1");
    }

    [Fact]
    public void ServerInfo_ShouldHaveDefaultValues()
    {
        var serverInfo = new ServerInfo();

        serverInfo.ProtocolVersion.Should().Be(McpConstants.ProtocolVersion);
        serverInfo.Capabilities.Should().NotBeNull();
        serverInfo.ServerMetadata.Should().NotBeNull();
        serverInfo.ServerMetadata.Name.Should().Be("UnnamedServer");
        serverInfo.ServerMetadata.Version.Should().Be("0.0.0");
    }

    [Fact]
    public void ToolResponse_ShouldInitializeContentList()
    {
        var response = new ToolResponse();
        response.Content.Should().NotBeNull();
        response.Content.Should().BeEmpty();
        response.IsError.Should().BeNull();
    }

    [Fact]
    public void ContentPart_ShouldHaveDefaultType()
    {
        var content = new ContentPart { Text = "Hello" };
        content.Type.Should().Be("text");
        content.Text.Should().Be("Hello");
    }
}
