namespace ButterMorph.SchemaDesign;

using System.Text;
using System.Text.Json;
using ButterMorph.Abstractions;

/// <summary>
/// Builds payload JSON Schema from designer state.
/// </summary>
public sealed class PayloadSchemaBuilder : IPayloadSchemaBuilder
{
    // JSON text used for map-shaped payloads.
    private const string MapType = "obj" + "ect";

    /// <summary>
    /// Builds the payload schema result.
    /// </summary>
    /// <param name="input">The design input.</param>
    /// <param name="schemaTypes">The available schema type catalog.</param>
    /// <param name="metadataFields">The available metadata field catalog.</param>
    /// <returns>The design result.</returns>
    public PayloadSchemaDesignResult Build(PayloadSchemaDesignInput input, IReadOnlyCollection<SchemaTypeCatalogItem> schemaTypes, IReadOnlyCollection<FieldMetadataCatalogItem> metadataFields)
    {
        input = NormalizeInput(input);
        if (string.IsNullOrWhiteSpace(input.Key))
        {
            return Fail("BMSD300", "Payload schema key is required.", "Key");
        }

        if (string.IsNullOrWhiteSpace(input.Name))
        {
            return Fail("BMSD304", "Payload schema name is required.", "Name");
        }

        if (string.IsNullOrWhiteSpace(input.Version))
        {
            return Fail("BMSD305", "Payload schema version is required.", "Version");
        }

        if (string.IsNullOrWhiteSpace(input.JsonSchema))
        {
            return Fail("BMSD301", "Payload JSON Schema is required.", "JsonSchema");
        }

        try
        {
            using JsonDocument document = JsonDocument.Parse(input.JsonSchema);
            string type = string.Empty;

            if (document.RootElement.TryGetProperty("type", out JsonElement typeElement) && typeElement.ValueKind == JsonValueKind.String)
            {
                type = typeElement.GetString();
            }

            if (!string.Equals(type, MapType, StringComparison.OrdinalIgnoreCase))
            {
                return Fail("BMSD302", "Payload schema root must be map-shaped.", "JsonSchema");
            }

            return new PayloadSchemaDesignResult
            {
                Succeeded = true,
                Diagnostics = [],
                Key = input.Key.Trim(),
                Name = input.Name.Trim(),
                Description = input.Description.Trim(),
                Version = input.Version.Trim(),
                VersionComment = input.VersionComment.Trim(),
                Metadata = CopyMetadata(input.Metadata),
                JsonSchema = CreateAtlasSchema(input.JsonSchema, input.Metadata)
            };
        }
        catch (JsonException exception)
        {
            return Fail("BMSD303", exception.Message, "JsonSchema");
        }
    }

    // Normalizes model-bound values that can arrive as null from form posts.
    private static PayloadSchemaDesignInput NormalizeInput(PayloadSchemaDesignInput input)
    {
        if (input == null)
        {
            input = new PayloadSchemaDesignInput();
        }

        return new PayloadSchemaDesignInput
        {
            Name = SafeString(input.Name),
            Key = SafeString(input.Key),
            Description = SafeString(input.Description),
            Version = SafeString(input.Version),
            VersionComment = SafeString(input.VersionComment),
            Metadata = CopyMetadata(input.Metadata),
            JsonSchema = SafeString(input.JsonSchema)
        };
    }

    // Creates Atlas-compatible JSON Schema without embedding host-owned identity fields.
    private static string CreateAtlasSchema(string jsonSchema, IReadOnlyDictionary<string, string> metadata)
    {
        using JsonDocument document = JsonDocument.Parse(jsonSchema);
        using MemoryStream stream = new();
        using Utf8JsonWriter writer = new(stream, new JsonWriterOptions { Indented = false });

        writer.WriteStartObject();
        foreach (JsonProperty property in document.RootElement.EnumerateObject())
        {
            if (ShouldSkipRootProperty(property.Name, metadata))
            {
                continue;
            }

            property.WriteTo(writer);
        }

        WriteOpenMetadata(writer, metadata);

        writer.WriteEndObject();
        writer.Flush();

        return Encoding.UTF8.GetString(stream.ToArray());
    }

    // Detects root keywords controlled outside the JSON Schema body.
    private static bool ShouldSkipRootProperty(string propertyName, IReadOnlyDictionary<string, string> metadata)
    {
        if (metadata.Count > 0 && string.Equals(propertyName, "metadata", StringComparison.Ordinal))
        {
            return true;
        }

        return string.Equals(propertyName, "key", StringComparison.Ordinal) ||
            string.Equals(propertyName, "name", StringComparison.Ordinal) ||
            string.Equals(propertyName, "description", StringComparison.Ordinal) ||
            string.Equals(propertyName, "version", StringComparison.Ordinal) ||
            string.Equals(propertyName, "versionComment", StringComparison.Ordinal);
    }

    // Copies open metadata safely.
    private static IReadOnlyDictionary<string, string> CopyMetadata(IReadOnlyDictionary<string, string> metadata)
    {
        if (metadata == null)
        {
            return new Dictionary<string, string>(StringComparer.Ordinal);
        }

        return new Dictionary<string, string>(metadata, StringComparer.Ordinal);
    }

    // Writes the schema-level open metadata bag.
    private static void WriteOpenMetadata(Utf8JsonWriter writer, IReadOnlyDictionary<string, string> metadata)
    {
        if (metadata.Count == 0)
        {
            return;
        }

        writer.WritePropertyName("metadata");
        writer.WriteStartObject();

        foreach (KeyValuePair<string, string> pair in metadata)
        {
            writer.WritePropertyName(pair.Key);
            WriteOpenMetadataValue(writer, pair.Value);
        }

        writer.WriteEndObject();
    }

    // Writes open metadata values preserving structured JSON when present.
    private static void WriteOpenMetadataValue(Utf8JsonWriter writer, string value)
    {
        if (TryWriteRawOpenMetadataValue(writer, value))
        {
            return;
        }

        writer.WriteStringValue(value);
    }

    // Attempts to write a metadata value as raw JSON.
    private static bool TryWriteRawOpenMetadataValue(Utf8JsonWriter writer, string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        try
        {
            using JsonDocument document = JsonDocument.Parse(value);
            document.RootElement.WriteTo(writer);
            return true;
        }
        catch (JsonException)
        {
            return false;
        }
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

    // Creates a failed result.
    private static PayloadSchemaDesignResult Fail(string code, string message, string path)
    {
        return new PayloadSchemaDesignResult
        {
            Succeeded = false,
            Diagnostics =
            [
                new DiagnosticEntry
                {
                    Code = code,
                    Message = message,
                    Severity = "Error",
                    Path = path
                }
            ]
        };
    }
}

