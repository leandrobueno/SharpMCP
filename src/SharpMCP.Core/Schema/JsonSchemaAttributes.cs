using System.Text.Json.Serialization;

namespace SharpMCP.Core.Schema;

/// <summary>
/// Marks a class as a JSON Schema type.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = false)]
public class JsonSchemaAttribute : Attribute
{
    /// <summary>
    /// Gets or sets the schema title.
    /// </summary>
    public string? Title { get; set; }

    /// <summary>
    /// Gets or sets the schema description.
    /// </summary>
    public string? Description { get; set; }
}

/// <summary>
/// Marks a property as required in the JSON schema.
/// </summary>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
public class JsonRequiredAttribute : Attribute
{
}

/// <summary>
/// Provides a description for a property in the JSON schema.
/// </summary>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
public class JsonDescriptionAttribute : Attribute
{
    /// <summary>
    /// Gets the description text.
    /// </summary>
    public string Description { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="JsonDescriptionAttribute"/> class.
    /// </summary>
    /// <param name="description">The description text.</param>
    public JsonDescriptionAttribute(string description)
    {
        Description = description;
    }
}

/// <summary>
/// Specifies constraints for string properties.
/// </summary>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
public class JsonStringConstraintsAttribute : Attribute
{
    /// <summary>
    /// Gets or sets the minimum length.
    /// </summary>
    public int MinLength { get; set; } = -1;

    /// <summary>
    /// Gets or sets the maximum length.
    /// </summary>
    public int MaxLength { get; set; } = -1;

    /// <summary>
    /// Gets or sets the regex pattern.
    /// </summary>
    public string? Pattern { get; set; }

    /// <summary>
    /// Gets or sets the format (e.g., "email", "uri", "date-time").
    /// </summary>
    public string? Format { get; set; }
}

/// <summary>
/// Specifies constraints for numeric properties.
/// </summary>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
public class JsonNumberConstraintsAttribute : Attribute
{
    /// <summary>
    /// Gets or sets the minimum value.
    /// </summary>
    public double Minimum { get; set; } = double.MinValue;

    /// <summary>
    /// Gets or sets the maximum value.
    /// </summary>
    public double Maximum { get; set; } = double.MaxValue;

    /// <summary>
    /// Gets or sets whether the minimum is exclusive.
    /// </summary>
    public bool ExclusiveMinimum { get; set; }

    /// <summary>
    /// Gets or sets whether the maximum is exclusive.
    /// </summary>
    public bool ExclusiveMaximum { get; set; }

    /// <summary>
    /// Gets or sets the multiple of constraint.
    /// </summary>
    public double MultipleOf { get; set; } = 0;

    /// <summary>
    /// Gets or sets whether the minimum constraint is set.
    /// </summary>
    public bool HasMinimum { get; set; }

    /// <summary>
    /// Gets or sets whether the maximum constraint is set.
    /// </summary>
    public bool HasMaximum { get; set; }

    /// <summary>
    /// Gets or sets whether the multiple of constraint is set.
    /// </summary>
    public bool HasMultipleOf { get; set; }
}

/// <summary>
/// Specifies constraints for array properties.
/// </summary>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
public class JsonArrayConstraintsAttribute : Attribute
{
    /// <summary>
    /// Gets or sets the minimum number of items.
    /// </summary>
    public int MinItems { get; set; } = -1;

    /// <summary>
    /// Gets or sets the maximum number of items.
    /// </summary>
    public int MaxItems { get; set; } = -1;

    /// <summary>
    /// Gets or sets whether items must be unique.
    /// </summary>
    public bool UniqueItems { get; set; }
}

/// <summary>
/// Specifies enum values for a property.
/// </summary>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
public class JsonEnumAttribute : Attribute
{
    /// <summary>
    /// Gets the allowed values.
    /// </summary>
    public object[] Values { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="JsonEnumAttribute"/> class.
    /// </summary>
    /// <param name="values">The allowed values.</param>
    public JsonEnumAttribute(params object[] values)
    {
        Values = values;
    }
}

/// <summary>
/// Represents a JSON Schema object.
/// </summary>
public class JsonSchema
{
    /// <summary>
    /// Gets or sets the schema type.
    /// </summary>
    [JsonPropertyName("type")]
    public string Type { get; set; } = "object";

    /// <summary>
    /// Gets or sets the schema title.
    /// </summary>
    [JsonPropertyName("title")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Title { get; set; }

    /// <summary>
    /// Gets or sets the schema description.
    /// </summary>
    [JsonPropertyName("description")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets the properties.
    /// </summary>
    [JsonPropertyName("properties")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public Dictionary<string, JsonSchema>? Properties { get; set; }

    /// <summary>
    /// Gets or sets the required properties.
    /// </summary>
    [JsonPropertyName("required")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<string>? Required { get; set; }

    /// <summary>
    /// Gets or sets additional properties allowed.
    /// </summary>
    [JsonPropertyName("additionalProperties")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public bool? AdditionalProperties { get; set; }

    /// <summary>
    /// Gets or sets the items schema (for arrays).
    /// </summary>
    [JsonPropertyName("items")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public JsonSchema? Items { get; set; }

    /// <summary>
    /// Gets or sets the enum values.
    /// </summary>
    [JsonPropertyName("enum")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public object[]? Enum { get; set; }

    /// <summary>
    /// Gets or sets the format.
    /// </summary>
    [JsonPropertyName("format")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Format { get; set; }

    /// <summary>
    /// Gets or sets the pattern.
    /// </summary>
    [JsonPropertyName("pattern")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Pattern { get; set; }

    /// <summary>
    /// Gets or sets the minimum length.
    /// </summary>
    [JsonPropertyName("minLength")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? MinLength { get; set; }

    /// <summary>
    /// Gets or sets the maximum length.
    /// </summary>
    [JsonPropertyName("maxLength")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? MaxLength { get; set; }

    /// <summary>
    /// Gets or sets the minimum value.
    /// </summary>
    [JsonPropertyName("minimum")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public double? Minimum { get; set; }

    /// <summary>
    /// Gets or sets the maximum value.
    /// </summary>
    [JsonPropertyName("maximum")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public double? Maximum { get; set; }

    /// <summary>
    /// Gets or sets the minimum items.
    /// </summary>
    [JsonPropertyName("minItems")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? MinItems { get; set; }

    /// <summary>
    /// Gets or sets the maximum items.
    /// </summary>
    [JsonPropertyName("maxItems")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? MaxItems { get; set; }

    /// <summary>
    /// Gets or sets whether items must be unique.
    /// </summary>
    [JsonPropertyName("uniqueItems")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public bool? UniqueItems { get; set; }
}
