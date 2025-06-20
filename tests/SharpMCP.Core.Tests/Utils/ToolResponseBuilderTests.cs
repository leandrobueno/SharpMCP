using SharpMCP.Core.Utils;
using Xunit;
using FluentAssertions;

namespace SharpMCP.Core.Tests.Utils;

public class ToolResponseBuilderTests
{
    [Fact]
    public void Create_ShouldReturnNewBuilder()
    {
        var builder = ToolResponseBuilder.Create();
        builder.Should().NotBeNull();
    }

    [Fact]
    public void WithContent_ShouldAddTextContent()
    {
        var response = ToolResponseBuilder.Create()
            .WithContent("Hello, world!")
            .Build();

        response.Content.Should().HaveCount(1);
        response.Content[0].Text.Should().Be("Hello, world!");
        response.Content[0].Type.Should().Be("text");
    }

    [Fact]
    public void WithContents_ShouldAddMultipleContents()
    {
        var response = ToolResponseBuilder.Create()
            .WithContents("First", "Second", "Third")
            .Build();

        response.Content.Should().HaveCount(3);
        response.Content[0].Text.Should().Be("First");
        response.Content[1].Text.Should().Be("Second");
        response.Content[2].Text.Should().Be("Third");
    }

    [Fact]
    public void WithFormattedContent_ShouldFormatText()
    {
        var response = ToolResponseBuilder.Create()
            .WithFormattedContent("Hello, {0}! You have {1} messages.", "Alice", 5)
            .Build();

        response.Content.Should().HaveCount(1);
        response.Content[0].Text.Should().Be("Hello, Alice! You have 5 messages.");
    }

    [Fact]
    public void WithError_ShouldMarkAsError()
    {
        var response = ToolResponseBuilder.Create()
            .WithError("Something went wrong")
            .Build();

        response.IsError.Should().BeTrue();
        response.Content.Should().HaveCount(1);
        response.Content[0].Text.Should().Be("Something went wrong");
    }

    [Fact]
    public void WithSuccess_ShouldMarkAsNotError()
    {
        var response = ToolResponseBuilder.Create()
            .WithSuccess("Operation completed successfully")
            .Build();

        response.IsError.Should().BeFalse();
        response.Content.Should().HaveCount(1);
        response.Content[0].Text.Should().Be("Operation completed successfully");
    }

    [Fact]
    public void Success_StaticMethod_ShouldCreateSuccessResponse()
    {
        var response = ToolResponseBuilder.Success("All good!");

        response.IsError.Should().BeFalse();
        response.Content.Should().HaveCount(1);
        response.Content[0].Text.Should().Be("All good!");
    }

    [Fact]
    public void Error_StaticMethod_ShouldCreateErrorResponse()
    {
        var response = ToolResponseBuilder.Error("Failed!");

        response.IsError.Should().BeTrue();
        response.Content.Should().HaveCount(1);
        response.Content[0].Text.Should().Be("Failed!");
    }
}
