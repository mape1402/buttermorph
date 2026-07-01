namespace ButterMorph.SchemaDesign;

using System.Text.Json;

/// <summary>
/// Hydrates editable schema type input from saved schema type definitions.
/// </summary>
public sealed class SchemaTypeDefinitionHydrator : ISchemaTypeDefinitionHydrator
{
    /// <inheritdoc />
    public SchemaTypeDesignInput Hydrate(SchemaTypeDefinition definition)
    {
        SchemaTypeDesignInput input = new()
        {
            Key = definition.Key,
            Name = definition.Name,
            Description = definition.Description,
            VersionNumber = definition.Version,
            BaseType = string.IsNullOrWhiteSpace(definition.BaseType) ? "string" : definition.BaseType,
            Comment = definition.Comment,
            PayloadSchemaJson = ResolveSchemaJson(definition)
        };

        ApplySchema(input, input.PayloadSchemaJson);
        return input;
    }

    private static string ResolveSchemaJson(SchemaTypeDefinition definition)
    {
        if (definition.Schema.ValueKind != JsonValueKind.Undefined)
        {
            return definition.Schema.GetRawText();
        }

        return definition.JsonSchema;
    }

    private static void ApplySchema(SchemaTypeDesignInput input, string schemaJson)
    {
        if (string.IsNullOrWhiteSpace(schemaJson))
        {
            return;
        }

        try
        {
            using JsonDocument document = JsonDocument.Parse(schemaJson);
            JsonElement root = document.RootElement;
            if (root.TryGetProperty("type", out JsonElement typeElement))
            {
                input.BaseType = typeElement.ToString();
            }

            input.MinLength = ReadValue(root, "minLength");
            input.MaxLength = ReadValue(root, "maxLength");
            input.Pattern = ReadValue(root, "pattern");
            input.Minimum = ReadValue(root, "minimum");
            input.Maximum = ReadValue(root, "maximum");
            input.Precision = ReadValue(root, "precision");
            input.Scale = ReadValue(root, "scale");
            input.MinItems = ReadValue(root, "minItems");
            input.MaxItems = ReadValue(root, "maxItems");
            input.AllowedValuesJson = ReadEnum(root);

            if (root.TryGetProperty("items", out JsonElement items))
            {
                ApplyArrayItem(input, items);
            }
        }
        catch (JsonException)
        {
            input.PayloadSchemaJson = schemaJson;
        }
    }

    private static void ApplyArrayItem(SchemaTypeDesignInput input, JsonElement items)
    {
        input.ArrayItemType = ReadValue(items, "type");
        if (string.IsNullOrWhiteSpace(input.ArrayItemType))
        {
            input.ArrayItemType = "string";
        }

        if (string.Equals(input.ArrayItemType, "object", StringComparison.OrdinalIgnoreCase) &&
            items.TryGetProperty("properties", out JsonElement properties))
        {
            input.PayloadSchemaJson = "{\"type\":\"object\",\"properties\":" + properties.GetRawText() + "}";
        }
    }

    private static string ReadValue(JsonElement root, string propertyName)
    {
        if (root.TryGetProperty(propertyName, out JsonElement element))
        {
            return element.ToString();
        }

        return string.Empty;
    }

    private static string ReadEnum(JsonElement root)
    {
        if (root.TryGetProperty("enum", out JsonElement element) &&
            element.ValueKind == JsonValueKind.Array)
        {
            return element.GetRawText();
        }

        return "[]";
    }
}
