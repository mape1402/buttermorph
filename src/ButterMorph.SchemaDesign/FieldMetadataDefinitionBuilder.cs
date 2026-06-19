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
                SortOrder = input.SortOrder
            };
        }

        return new FieldMetadataDesignResult
        {
            Succeeded = true,
            Diagnostics = [],
            Name = input.Name.Trim(),
            Key = input.Key.Trim(),
            Description = input.Description.Trim(),
            DataType = input.DataType.Trim(),
            AppliesToJson = WriteStringArray(SchemaDesignJsonTools.ReadLines(input.AppliesTo)),
            IsRequired = input.IsRequired,
            IsActive = input.IsActive,
            SortOrder = input.SortOrder,
            ValidationJson = CreateValidationJson(input)
        };
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

        return diagnostics;
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
