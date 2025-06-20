using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using SharpMCP.Core.Protocol;
using SharpMCP.Core.Tools;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using System.Text.Json.Serialization;
using Xunit;

namespace SharpMCP.Server.Tests.Tools;

/// <summary>
/// Tests for the McpToolBase class.
/// </summary>
public class McpToolBaseTests
{
    private readonly Mock<ILogger> _loggerMock = new();

    [Fact]
    public async Task ExecuteAsync_Should_Execute_Tool_Successfully()
    {
        // Arrange
        var tool = new TestTool();
        var arguments = JsonDocument.Parse(@"{""input"": ""test value""}");

        // Act
        var result = await tool.ExecuteAsync(arguments.RootElement);

        // Assert
        result.Should().NotBeNull();
        result.Content.Should().NotBeNull();
        result.Content[0].Text.Should().Be("Processed: test value");
    }

    [Fact]
    public async Task ExecuteAsync_Should_Handle_Missing_Arguments()
    {
        // Arrange
        var tool = new TestTool();

        // Act
        var result = await tool.ExecuteAsync(null);

        // Assert
        result.Should().NotBeNull();
        result.Content.Should().NotBeNull();
        result.Content[0].Text.Should().Be("Processed: ");
    }

    [Fact]
    public void GetInputSchema_Should_Return_Valid_Schema()
    {
        // Arrange
        var tool = new TestTool();

        // Act
        var schema = tool.GetInputSchema();

        // Assert
        schema.ValueKind.Should().Be(JsonValueKind.Object);
        schema.GetProperty("type").GetString().Should().Be("object");
        schema.GetProperty("properties").Should().NotBeNull();
    }

    [Fact]
    public void Name_Should_Return_Tool_Name()
    {
        // Arrange
        var tool = new TestTool();

        // Act & Assert
        tool.Name.Should().Be("test_tool");
    }

    [Fact]
    public void Description_Should_Return_Tool_Description()
    {
        // Arrange
        var tool = new TestTool();

        // Act & Assert
        tool.Description.Should().Be("A test tool for unit testing");
    }

    [Fact]
    public async Task ExecuteAsync_Should_Validate_Arguments()
    {
        // Arrange
        var tool = new ValidatingTestTool();
        var arguments = JsonDocument.Parse(@"{""requiredField"": """"}");

        // Act
        var act = () => tool.ExecuteAsync(arguments.RootElement);

        // Assert
        await act.Should().ThrowAsync<McpToolException>()
            .WithMessage("*validation failed*");
    }

    [Fact]
    public async Task NoArgsTool_Should_Work_Without_Arguments()
    {
        // Arrange
        var tool = new NoArgsTestTool();

        // Act
        var result = await tool.ExecuteAsync(null);

        // Assert
        result.Should().NotBeNull();
        result.Content[0].Text.Should().Be("No args tool executed");
    }

    private class TestTool : McpToolBase<TestToolArguments>
    {
        public override string Name => "test_tool";
        public override string? Description => "A test tool for unit testing";

        protected override Task<ToolResponse> ExecuteAsync(TestToolArguments args, CancellationToken cancellationToken)
        {
            return Task.FromResult(Success($"Processed: {args.Input ?? ""}"));
        }
    }

    private class ValidatingTestTool : McpToolBase<ValidatingTestToolArguments>
    {
        public override string Name => "validating_tool";
        public override string? Description => "A tool with validation";

        protected override Task<ToolResponse> ExecuteAsync(ValidatingTestToolArguments args, CancellationToken cancellationToken)
        {
            return Task.FromResult(Success("Valid"));
        }

        protected override string? ValidateArguments(ValidatingTestToolArguments args)
        {
            if (string.IsNullOrEmpty(args.RequiredField))
            {
                return "RequiredField cannot be empty";
            }
            return null;
        }
    }

    private class NoArgsTestTool : McpToolBase
    {
        public override string Name => "no_args_tool";
        public override string? Description => "A tool without arguments";

        protected override Task<ToolResponse> ExecuteAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult(Success("No args tool executed"));
        }
    }

    private class TestToolArguments
    {
        [Description("Input value to process")]
        [JsonPropertyName("input")]
        public string Input { get; set; } = "";
    }

    private class ValidatingTestToolArguments
    {
        [Required]
        public string RequiredField { get; set; } = "";
    }
}
