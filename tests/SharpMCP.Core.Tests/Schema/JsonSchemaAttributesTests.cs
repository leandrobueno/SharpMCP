using SharpMCP.Core.Schema;
using System.Reflection;
using Xunit;
using FluentAssertions;

namespace SharpMCP.Core.Tests.Schema;

public class JsonSchemaAttributesTests
{
    [JsonSchema(Title = "Test Schema", Description = "A test schema")]
    public class TestClass
    {
        [JsonRequired]
        [JsonDescription("A test property")]
        public string? TestProperty { get; set; }

        [JsonStringConstraints(MinLength = 5, MaxLength = 10, Pattern = @"^\d+$")]
        public string? ConstrainedString { get; set; }

        [JsonNumberConstraints(Minimum = 0, Maximum = 100, HasMinimum = true, HasMaximum = true)]
        public int ConstrainedNumber { get; set; }

        [JsonArrayConstraints(MinItems = 1, MaxItems = 5, UniqueItems = true)]
        public string[]? ConstrainedArray { get; set; }

        [JsonEnum("option1", "option2", "option3")]
        public string? EnumProperty { get; set; }
    }

    [Fact]
    public void JsonSchemaAttribute_ShouldHaveCorrectProperties()
    {
        var type = typeof(TestClass);
        var attribute = type.GetCustomAttribute<JsonSchemaAttribute>();

        attribute.Should().NotBeNull();
        attribute!.Title.Should().Be("Test Schema");
        attribute.Description.Should().Be("A test schema");
    }

    [Fact]
    public void JsonRequiredAttribute_ShouldBeAppliedToProperty()
    {
        var property = typeof(TestClass).GetProperty(nameof(TestClass.TestProperty));
        var attribute = property!.GetCustomAttribute<JsonRequiredAttribute>();

        attribute.Should().NotBeNull();
    }

    [Fact]
    public void JsonDescriptionAttribute_ShouldHaveCorrectDescription()
    {
        var property = typeof(TestClass).GetProperty(nameof(TestClass.TestProperty));
        var attribute = property!.GetCustomAttribute<JsonDescriptionAttribute>();

        attribute.Should().NotBeNull();
        attribute!.Description.Should().Be("A test property");
    }

    [Fact]
    public void JsonStringConstraintsAttribute_ShouldHaveCorrectValues()
    {
        var property = typeof(TestClass).GetProperty(nameof(TestClass.ConstrainedString));
        var attribute = property!.GetCustomAttribute<JsonStringConstraintsAttribute>();

        attribute.Should().NotBeNull();
        attribute!.MinLength.Should().Be(5);
        attribute.MaxLength.Should().Be(10);
        attribute.Pattern.Should().Be(@"^\d+$");
    }

    [Fact]
    public void JsonNumberConstraintsAttribute_ShouldHaveCorrectValues()
    {
        var property = typeof(TestClass).GetProperty(nameof(TestClass.ConstrainedNumber));
        var attribute = property!.GetCustomAttribute<JsonNumberConstraintsAttribute>();

        attribute.Should().NotBeNull();
        attribute!.Minimum.Should().Be(0);
        attribute.Maximum.Should().Be(100);
        attribute.HasMinimum.Should().BeTrue();
        attribute.HasMaximum.Should().BeTrue();
    }

    [Fact]
    public void JsonArrayConstraintsAttribute_ShouldHaveCorrectValues()
    {
        var property = typeof(TestClass).GetProperty(nameof(TestClass.ConstrainedArray));
        var attribute = property!.GetCustomAttribute<JsonArrayConstraintsAttribute>();

        attribute.Should().NotBeNull();
        attribute!.MinItems.Should().Be(1);
        attribute.MaxItems.Should().Be(5);
        attribute.UniqueItems.Should().BeTrue();
    }

    [Fact]
    public void JsonEnumAttribute_ShouldHaveCorrectValues()
    {
        var property = typeof(TestClass).GetProperty(nameof(TestClass.EnumProperty));
        var attribute = property!.GetCustomAttribute<JsonEnumAttribute>();

        attribute.Should().NotBeNull();
        attribute!.Values.Should().BeEquivalentTo(new[] { "option1", "option2", "option3" });
    }

    [Fact]
    public void JsonSchema_ShouldSerializeCorrectly()
    {
        var schema = new JsonSchema
        {
            Type = "object",
            Title = "Test Schema",
            Description = "A test schema",
            Properties = new Dictionary<string, JsonSchema>
            {
                ["name"] = new JsonSchema
                {
                    Type = "string",
                    MinLength = 1,
                    MaxLength = 100
                }
            },
            Required = ["name"],
            AdditionalProperties = false
        };

        var json = System.Text.Json.JsonSerializer.Serialize(schema);

        json.Should().Contain("\"type\":\"object\"");
        json.Should().Contain("\"title\":\"Test Schema\"");
        json.Should().Contain("\"required\":[\"name\"]");
        json.Should().Contain("\"additionalProperties\":false");
    }
}
