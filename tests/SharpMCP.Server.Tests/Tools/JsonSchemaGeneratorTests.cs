using System.ComponentModel;
using System.Text.Json.Serialization;
using FluentAssertions;
using SharpMCP.Core.Schema;
using SharpMCP.Core.Utils;
using Xunit;

namespace SharpMCP.Server.Tests.Tools;

/// <summary>
/// Tests for the JsonSchemaGenerator class.
/// </summary>
public class JsonSchemaGeneratorTests
{
    [Fact]
    public void GenerateSchema_Should_Handle_Simple_Types()
    {
        // Act
        var schema = JsonSchemaGenerator.GenerateSchema<SimpleType>();

        // Assert
        schema.Type.Should().Be("object");
        schema.Properties.Should().ContainKey("Name");
        schema.Properties.Should().ContainKey("Age");
        schema.Properties.Should().ContainKey("IsActive");

        schema.Properties["Name"].Type.Should().Be("string");
        schema.Properties["Age"].Type.Should().Be("integer");
        schema.Properties["IsActive"].Type.Should().Be("boolean");
    }

    [Fact]
    public void GenerateSchema_Should_Include_Descriptions()
    {
        // Act
        var schema = JsonSchemaGenerator.GenerateSchema<TypeWithDescription>();

        // Assert
        schema.Properties!["Name"].Description.Should().Be("The person's name");
    }

    [Fact]
    public void GenerateSchema_Should_Handle_Required_Properties()
    {
        // Act
        var schema = JsonSchemaGenerator.GenerateSchema<TypeWithRequired>();

        // Assert
        schema.Required.Should().NotBeNull();
        schema.Required.Should().Contain("RequiredField");
        schema.Required.Should().Contain("Age"); // Value types are required by default
    }

    [Fact]
    public void GenerateSchema_Should_Handle_Validation_Attributes()
    {
        // Act
        var schema = JsonSchemaGenerator.GenerateSchema<TypeWithValidation>();

        // Assert
        var rangeField = schema.Properties!["RangeField"];
        rangeField.Minimum.Should().Be(1);
        rangeField.Maximum.Should().Be(100);

        var lengthField = schema.Properties["LengthField"];
        lengthField.MinLength.Should().Be(5);
        lengthField.MaxLength.Should().Be(10);
    }

    [Fact]
    public void GenerateSchema_Should_Handle_Nested_Types()
    {
        // Act
        var schema = JsonSchemaGenerator.GenerateSchema<NestedType>();

        // Assert
        schema.Properties!["Inner"].Type.Should().Be("object");
        schema.Properties["Inner"].Properties.Should().ContainKey("Value");
        schema.Properties["Inner"].Properties!["Value"].Type.Should().Be("string");
    }

    [Fact]
    public void GenerateSchema_Should_Handle_Arrays()
    {
        // Act
        var schema = JsonSchemaGenerator.GenerateSchema<TypeWithArray>();

        // Assert
        schema.Properties!["Items"].Type.Should().Be("array");
        schema.Properties["Items"].Items.Should().NotBeNull();
        schema.Properties["Items"].Items!.Type.Should().Be("string");
    }

    [Fact]
    public void GenerateSchema_Should_Handle_Enums()
    {
        // Act
        var schema = JsonSchemaGenerator.GenerateSchema<TypeWithEnum>();

        // Assert
        schema.Properties!["Status"].Type.Should().Be("string");
        schema.Properties["Status"].Enum.Should().NotBeNull();
        schema.Properties["Status"].Enum.Should().Contain("Active");
        schema.Properties["Status"].Enum.Should().Contain("Inactive");
    }

    [Fact]
    public void GenerateSchema_Should_Ignore_JsonIgnore_Properties()
    {
        // Act
        var schema = JsonSchemaGenerator.GenerateSchema<TypeWithIgnored>();

        // Assert
        schema.Properties.Should().ContainKey("VisibleProperty");
        schema.Properties.Should().NotContainKey("IgnoredProperty");
    }

    private class SimpleType
    {
        public string Name { get; set; } = "";
        public int Age { get; set; }
        public bool IsActive { get; set; }
    }

    private class TypeWithDescription
    {
        [Description("The person's name")]
        public string Name { get; set; } = "";
    }

    private class TypeWithRequired
    {
        [SharpMCP.Core.Schema.JsonRequired]
        public string RequiredField { get; set; } = "";
        public string? OptionalField { get; set; }
        public int Age { get; set; } // Value type, implicitly required
    }

    private class TypeWithValidation
    {
        [JsonNumberConstraints(Minimum = 1, Maximum = 100, HasMinimum = true, HasMaximum = true)]
        public int RangeField { get; set; }

        [JsonStringConstraints(MinLength = 5, MaxLength = 10)]
        public string LengthField { get; set; } = "";
    }

    private class NestedType
    {
        public InnerType Inner { get; set; } = new();
    }

    private class InnerType
    {
        public string Value { get; set; } = "";
    }

    private class TypeWithArray
    {
        public string[] Items { get; set; } = Array.Empty<string>();
    }

    private class TypeWithEnum
    {
        public StatusEnum Status { get; set; }
    }

    private enum StatusEnum
    {
        Active,
        Inactive
    }

    private class TypeWithIgnored
    {
        public string VisibleProperty { get; set; } = "";

        [JsonIgnore]
        public string IgnoredProperty { get; set; } = "";
    }
}
