namespace ButterMorph.SchemaDesign;

using System.Text.Json;

/// <summary>
/// Hydrates editable custom field input from saved custom field definitions.
/// </summary>
public sealed class FieldMetadataDefinitionHydrator : IFieldMetadataDefinitionHydrator
{
    /// <inheritdoc />
    public FieldMetadataDesignInput Hydrate(CustomFieldDefinition definition)
    {
        FieldMetadataDesignInput input = new()
        {
            Key = definition.Key,
            Name = definition.Name,
            Description = definition.Description,
            Version = definition.Version,
            VersionComment = definition.VersionComment,
            DataType = definition.DataType,
            AppliesTo = string.Join(Environment.NewLine, definition.AppliesTo ?? []),
            IsRequired = definition.IsRequired,
            IsActive = definition.IsActive,
            ChildrenDefinitionJson = SerializeElement(definition.ChildrenDefinition),
            ArrayItemDataType = definition.ArrayItemDataType ?? string.Empty,
            ArrayItemDefinitionJson = SerializeElement(definition.ArrayItemDefinition)
        };

        ApplyValidation(input, definition.Validation);
        return input;
    }

    private static void ApplyValidation(FieldMetadataDesignInput input, IReadOnlyDictionary<string, JsonElement> validation)
    {
        if (validation == null || validation.Count == 0)
        {
            return;
        }

        input.MinLength = ReadValue(validation, "minLength");
        input.MaxLength = ReadValue(validation, "maxLength");
        input.Pattern = ReadValue(validation, "pattern");
        input.Minimum = ReadValue(validation, "minimum");
        input.Maximum = ReadValue(validation, "maximum");
        input.DateMinimum = ReadValue(validation, "dateMinimum");
        input.DateMaximum = ReadValue(validation, "dateMaximum");
        input.AllowedValues = ReadAllowedValues(validation);
    }

    private static string ReadValue(IReadOnlyDictionary<string, JsonElement> validation, string key)
    {
        if (validation.TryGetValue(key, out JsonElement value))
        {
            return value.ToString();
        }

        return string.Empty;
    }

    private static string ReadAllowedValues(IReadOnlyDictionary<string, JsonElement> validation)
    {
        if (!validation.TryGetValue("allowedValues", out JsonElement values) ||
            values.ValueKind != JsonValueKind.Array)
        {
            return string.Empty;
        }

        List<string> lines = [];
        foreach (JsonElement value in values.EnumerateArray())
        {
            lines.Add(value.ToString());
        }

        return string.Join(Environment.NewLine, lines);
    }

    private static string SerializeElement(JsonElement element)
    {
        if (element.ValueKind == JsonValueKind.Undefined)
        {
            return string.Empty;
        }

        return element.GetRawText();
    }
}
