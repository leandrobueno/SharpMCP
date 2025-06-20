using System.ComponentModel;
using System.Reflection;
using System.Text.Json.Serialization;
using SharpMCP.Core.Schema;

namespace SharpMCP.Core.Utils;

/// <summary>
/// Generates JSON Schema from C# types using reflection and attributes.
/// </summary>
public static class JsonSchemaGenerator
{
    /// <summary>
    /// Generates a JSON Schema for the specified type.
    /// </summary>
    /// <typeparam name="T">The type to generate schema for.</typeparam>
    /// <returns>The generated JSON Schema.</returns>
    public static JsonSchema GenerateSchema<T>()
    {
        return GenerateSchema(typeof(T));
    }

    /// <summary>
    /// Generates a JSON Schema for the specified type.
    /// </summary>
    /// <param name="type">The type to generate schema for.</param>
    /// <returns>The generated JSON Schema.</returns>
    public static JsonSchema GenerateSchema(Type type)
    {
        var schema = new JsonSchema();

        // Handle nullable types
        var underlyingType = Nullable.GetUnderlyingType(type);
        if (underlyingType != null)
        {
            type = underlyingType;
        }

        // Determine JSON type
        if (type == typeof(string))
        {
            schema.Type = "string";
            ApplyStringConstraints(schema, type);
        }
        else if (type == typeof(bool))
        {
            schema.Type = "boolean";
        }
        else if (IsNumericType(type))
        {
            schema.Type = IsIntegerType(type) ? "integer" : "number";
            ApplyNumberConstraints(schema, type);
        }
        else if (type.IsArray || (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>)))
        {
            schema.Type = "array";
            var elementType = type.IsArray ? type.GetElementType()! : type.GetGenericArguments()[0];
            schema.Items = GenerateSchema(elementType);
            ApplyArrayConstraints(schema, type);
        }
        else if (type.IsEnum)
        {
            schema.Type = "string";
            schema.Enum = Enum.GetNames(type).Cast<object>().ToArray();
        }
        else if (type.IsClass || type.IsValueType)
        {
            schema.Type = "object";
            schema.Properties = [];
            schema.Required = [];

            foreach (var property in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                if (property.GetCustomAttribute<JsonIgnoreAttribute>() != null)
                {
                    continue;
                }


                var propertyName = GetPropertyName(property);
                var propertySchema = GeneratePropertySchema(property);

                schema.Properties[propertyName] = propertySchema;

                // Check if property is required
                if (property.GetCustomAttribute<Core.Schema.JsonRequiredAttribute>() != null ||
                    property.GetCustomAttribute<System.Text.Json.Serialization.JsonRequiredAttribute>() != null ||
                    (property.PropertyType.IsValueType && Nullable.GetUnderlyingType(property.PropertyType) == null))
                {
                    schema.Required.Add(propertyName);
                }
            }

            if (schema.Required.Count == 0)
            {
                schema.Required = null;
            }

        }

        // Apply description from attributes
        var descriptionAttr = type.GetCustomAttribute<DescriptionAttribute>();
        var jsonDescriptionAttr = type.GetCustomAttribute<JsonDescriptionAttribute>();
        if (descriptionAttr != null)
        {
            schema.Description = descriptionAttr.Description;
        }
        else if (jsonDescriptionAttr != null)
        {
            schema.Description = jsonDescriptionAttr.Description;
        }

        return schema;
    }

    private static JsonSchema GeneratePropertySchema(PropertyInfo property)
    {
        var schema = GenerateSchema(property.PropertyType);

        // Apply property-specific attributes
        var descriptionAttr = property.GetCustomAttribute<DescriptionAttribute>();
        var jsonDescriptionAttr = property.GetCustomAttribute<JsonDescriptionAttribute>();
        if (descriptionAttr != null)
        {
            schema.Description = descriptionAttr.Description;
        }
        else if (jsonDescriptionAttr != null)
        {
            schema.Description = jsonDescriptionAttr.Description;
        }

        // Apply constraints from property attributes
        ApplyPropertyConstraints(schema, property);

        return schema;
    }

    private static void ApplyPropertyConstraints(JsonSchema schema, PropertyInfo property)
    {
        // String constraints
        var stringConstraintsAttr = property.GetCustomAttribute<JsonStringConstraintsAttribute>();
        if (stringConstraintsAttr != null)
        {
            if (stringConstraintsAttr.MinLength >= 0)
            {
                schema.MinLength = stringConstraintsAttr.MinLength;
            }


            if (stringConstraintsAttr.MaxLength >= 0)
            {
                schema.MaxLength = stringConstraintsAttr.MaxLength;
            }


            schema.Pattern = stringConstraintsAttr.Pattern;
            schema.Format = stringConstraintsAttr.Format;
        }

        // Number constraints
        var numberConstraintsAttr = property.GetCustomAttribute<JsonNumberConstraintsAttribute>();
        if (numberConstraintsAttr != null)
        {
            if (numberConstraintsAttr.HasMinimum)
            {
                schema.Minimum = numberConstraintsAttr.Minimum;
            }

            if (numberConstraintsAttr.HasMaximum)
            {
                schema.Maximum = numberConstraintsAttr.Maximum;
            }

        }

        // Array constraints
        var arrayConstraintsAttr = property.GetCustomAttribute<JsonArrayConstraintsAttribute>();
        if (arrayConstraintsAttr != null)
        {
            if (arrayConstraintsAttr.MinItems >= 0)
            {
                schema.MinItems = arrayConstraintsAttr.MinItems;
            }

            if (arrayConstraintsAttr.MaxItems >= 0)
            {
                schema.MaxItems = arrayConstraintsAttr.MaxItems;
            }


            if (arrayConstraintsAttr.UniqueItems)
            {
                schema.UniqueItems = true;
            }

        }

        // Enum constraints
        var enumAttr = property.GetCustomAttribute<JsonEnumAttribute>();
        if (enumAttr != null)
        {
            schema.Enum = enumAttr.Values;
        }
    }

    private static void ApplyStringConstraints(JsonSchema schema, Type type)
    {
        // String constraints are typically applied at property level
    }

    private static void ApplyNumberConstraints(JsonSchema schema, Type type)
    {
        // Number constraints are typically applied at property level
    }

    private static void ApplyArrayConstraints(JsonSchema schema, Type type)
    {
        // Array constraints are typically applied at property level
    }

    private static string GetPropertyName(PropertyInfo property)
    {
        var jsonPropertyAttr = property.GetCustomAttribute<JsonPropertyNameAttribute>();
        return jsonPropertyAttr?.Name ?? property.Name;
    }

    private static bool IsNumericType(Type type)
    {
        return type == typeof(int) || type == typeof(long) || type == typeof(short) || type == typeof(byte) ||
               type == typeof(uint) || type == typeof(ulong) || type == typeof(ushort) || type == typeof(sbyte) ||
               type == typeof(float) || type == typeof(double) || type == typeof(decimal);
    }

    private static bool IsIntegerType(Type type)
    {
        return type == typeof(int) || type == typeof(long) || type == typeof(short) || type == typeof(byte) ||
               type == typeof(uint) || type == typeof(ulong) || type == typeof(ushort) || type == typeof(sbyte);
    }
}
