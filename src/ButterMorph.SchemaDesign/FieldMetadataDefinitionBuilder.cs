namespace ButterMorph.SchemaDesign;

using System.Text;
using System.Text.Json;
using ButterMorph.Abstractions;

/// <summary>
/// Builds field metadata definitions.
/// </summary>
public sealed class FieldMetadataDefinitionBuilder : IFieldMetadataDefinitionBuilder
{
    /// <summary>
    /// Builds the field metadata result.
    /// </summary>
    /// <param name="input">The design input.</param>
    /// <returns>The design result.</returns>
    public FieldMetadataDesignResult Build(FieldMetadataDesignInput input)
    {
        input = NormalizeInput(input);
        List<DiagnosticEntry> diagnostics = Validate(input);

        if (diagnostics.Count > 0)
        {
            return new FieldMetadataDesignResult
            {
                Succeeded = false,
                Diagnostics = diagnostics,
                Name = input.Name,
                Key = input.Key,
                Description = input.Description,
                DataType = input.DataType,
                IsRequired = input.IsRequired,
                IsActive = input.IsActive,
                ChildrenDefinitionJson = input.ChildrenDefinitionJson,
                ArrayItemDataType = input.ArrayItemDataType,
                ArrayItemDefinitionJson = input.ArrayItemDefinitionJson
            };
        }

        CustomFieldDefinition definition = new()
        {
            Name = input.Name.Trim(),
            Key = input.Key.Trim(),
            Description = input.Description.Trim(),
            DataType = input.DataType.Trim(),
            AppliesToJson = WriteStringArray(SchemaDesignJsonTools.ReadLines(input.AppliesTo)),
            IsRequired = input.IsRequired,
            IsActive = input.IsActive,
            ValidationJson = CreateValidationJson(input),
            ChildrenDefinitionJson = input.ChildrenDefinitionJson.Trim(),
            ArrayItemDataType = input.ArrayItemDataType.Trim(),
            ArrayItemDefinitionJson = input.ArrayItemDefinitionJson.Trim()
        };

        return new FieldMetadataDesignResult
        {
            Succeeded = true,
            Diagnostics = [],
            Definition = definition,
            Name = definition.Name,
            Key = definition.Key,
            Description = definition.Description,
            DataType = definition.DataType,
            AppliesToJson = definition.AppliesToJson,
            IsRequired = definition.IsRequired,
            IsActive = definition.IsActive,
            ValidationJson = definition.ValidationJson,
            ChildrenDefinitionJson = definition.ChildrenDefinitionJson,
            ArrayItemDataType = definition.ArrayItemDataType,
            ArrayItemDefinitionJson = definition.ArrayItemDefinitionJson
        };
    }

    // Normalizes model-bound values that can arrive as null from form posts.
    private static FieldMetadataDesignInput NormalizeInput(FieldMetadataDesignInput input)
    {
        if (input == null)
        {
            input = new FieldMetadataDesignInput();
        }

        return new FieldMetadataDesignInput
        {
            Name = SafeString(input.Name),
            Key = SafeString(input.Key),
            Description = SafeString(input.Description),
            DataType = SafeString(input.DataType),
            AppliesTo = SafeString(input.AppliesTo),
            IsRequired = input.IsRequired,
            IsActive = input.IsActive,
            MinLength = SafeString(input.MinLength),
            MaxLength = SafeString(input.MaxLength),
            Pattern = SafeString(input.Pattern),
            Minimum = SafeString(input.Minimum),
            Maximum = SafeString(input.Maximum),
            DateMinimum = SafeString(input.DateMinimum),
            DateMaximum = SafeString(input.DateMaximum),
            AllowedValues = SafeString(input.AllowedValues),
            ChildrenDefinitionJson = SafeString(input.ChildrenDefinitionJson),
            ArrayItemDataType = SafeString(input.ArrayItemDataType),
            ArrayItemDefinitionJson = SafeString(input.ArrayItemDefinitionJson)
        };
    }

    // Converts model-bound null strings into empty text.
    private static string SafeString(string value)
    {
        if (value == null)
        {
            return string.Empty;
        }

        return value;
    }

    // Validates required metadata fields.
    private static List<DiagnosticEntry> Validate(FieldMetadataDesignInput input)
    {
        List<DiagnosticEntry> diagnostics = [];

        if (string.IsNullOrWhiteSpace(input.Name))
        {
            diagnostics.Add(CreateDiagnostic("BMSD201", "Metadata name is required.", "Name"));
        }

        if (string.IsNullOrWhiteSpace(input.Key))
        {
            diagnostics.Add(CreateDiagnostic("BMSD202", "Metadata key is required.", "Key"));
        }

        if (string.IsNullOrWhiteSpace(input.DataType))
        {
            diagnostics.Add(CreateDiagnostic("BMSD203", "Metadata data type is required.", "DataType"));
        }

        if (string.Equals(input.DataType, "object", StringComparison.OrdinalIgnoreCase))
        {
            ValidateJsonDefinition(input.ChildrenDefinitionJson, diagnostics, "ChildrenDefinitionJson", "Object metadata fields are required.");
        }

        if (string.Equals(input.DataType, "array", StringComparison.OrdinalIgnoreCase))
        {
            if (string.IsNullOrWhiteSpace(input.ArrayItemDataType))
            {
                diagnostics.Add(CreateDiagnostic("BMSD204", "Array item type is required.", "ArrayItemDataType"));
            }

            if (string.Equals(input.ArrayItemDataType, "object", StringComparison.OrdinalIgnoreCase))
            {
                ValidateJsonDefinition(input.ArrayItemDefinitionJson, diagnostics, "ArrayItemDefinitionJson", "Array object item fields are required.");
            }
        }

        return diagnostics;
    }

    // Validates JSON schema fragments used by complex metadata fields.
    private static void ValidateJsonDefinition(string json, List<DiagnosticEntry> diagnostics, string path, string emptyMessage)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            diagnostics.Add(CreateDiagnostic("BMSD205", emptyMessage, path));
            return;
        }

        try
        {
            using JsonDocument document = JsonDocument.Parse(json);
            if (document.RootElement.ValueKind != JsonValueKind.Object)
            {
                diagnostics.Add(CreateDiagnostic("BMSD206", "Metadata structure must be a JSON object.", path));
            }
        }
        catch (JsonException exception)
        {
            diagnostics.Add(CreateDiagnostic("BMSD207", exception.Message, path));
        }
    }

    // Creates a diagnostic entry.
    private static DiagnosticEntry CreateDiagnostic(string code, string message, string path)
    {
        return new DiagnosticEntry
        {
            Code = code,
            Message = message,
            Severity = "Error",
            Path = path
        };
    }

    // Creates validation JSON.
    private static string CreateValidationJson(FieldMetadataDesignInput input)
    {
        using MemoryStream stream = new();
        using Utf8JsonWriter writer = new(stream, new JsonWriterOptions { Indented = false });
        writer.WriteStartObject();
        WriteString(writer, "dataType", input.DataType);
        WriteString(writer, "minLength", input.MinLength);
        WriteString(writer, "maxLength", input.MaxLength);
        WriteString(writer, "pattern", input.Pattern);
        WriteString(writer, "minimum", input.Minimum);
        WriteString(writer, "maximum", input.Maximum);
        WriteString(writer, "dateMinimum", input.DateMinimum);
        WriteString(writer, "dateMaximum", input.DateMaximum);
        IReadOnlyCollection<string> values = SchemaDesignJsonTools.ReadLines(input.AllowedValues);
        if (values.Count > 0)
        {
            writer.WritePropertyName("allowedValues");
            writer.WriteStartArray();
            foreach (string value in values)
            {
                writer.WriteStringValue(value);
            }
            writer.WriteEndArray();
        }
        writer.WriteEndObject();
        writer.Flush();

        return Encoding.UTF8.GetString(stream.ToArray());
    }

    // Writes a string array JSON document.
    private static string WriteStringArray(IReadOnlyCollection<string> values)
    {
        using MemoryStream stream = new();
        using Utf8JsonWriter writer = new(stream, new JsonWriterOptions { Indented = false });
        writer.WriteStartArray();
        foreach (string value in values)
        {
            writer.WriteStringValue(value);
        }
        writer.WriteEndArray();
        writer.Flush();

        return Encoding.UTF8.GetString(stream.ToArray());
    }

    // Writes optional string keyword.
    private static void WriteString(Utf8JsonWriter writer, string key, string value)
    {
        if (!string.IsNullOrWhiteSpace(value))
        {
            writer.WriteString(key, value.Trim());
        }
    }
}
