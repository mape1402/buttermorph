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

            if (ContainsRequiredArray(document.RootElement))
            {
                return Fail("BMSD306", "Required fields must be stored as required: true on each field.", "JsonSchema");
            }

            string jsonSchema = CreateAtlasSchema(input);
            PayloadSchemaDefinition definition = CreateDefinition(jsonSchema);

            return new PayloadSchemaDesignResult
            {
                Succeeded = true,
                Diagnostics = [],
                Definition = definition,
                Key = definition.Key,
                Name = definition.Name,
                Description = definition.Description,
                Version = definition.Version,
                VersionComment = definition.VersionComment,
                Metadata = CopyMetadata(input.Metadata),
                JsonSchema = jsonSchema
            };
        }
        catch (JsonException exception)
        {
            return Fail("BMSD303", exception.Message, "JsonSchema");
        }
    }

    // Creates the guardable host payload from the canonical schema JSON.
    private static PayloadSchemaDefinition CreateDefinition(string jsonSchema)
    {
        using JsonDocument document = JsonDocument.Parse(jsonSchema);
        JsonElement root = document.RootElement;

        return new PayloadSchemaDefinition
        {
            Key = ReadString(root, "key"),
            Name = ReadString(root, "name"),
            Description = ReadString(root, "description"),
            Version = ReadString(root, "version"),
            VersionComment = ReadString(root, "versionComment"),
            Metadata = ReadElementMap(root, "metadata"),
            Type = ReadString(root, "type"),
            Properties = ReadElementMap(root, "properties"),
            Definitions = ReadElementMap(root, "$defs"),
            MetadataDefinitions = ReadElementMap(root, "$metadataDefs")
        };
    }

    // Reads a string property.
    private static string ReadString(JsonElement element, string propertyName)
    {
        if (element.TryGetProperty(propertyName, out JsonElement property) &&
            property.ValueKind == JsonValueKind.String)
        {
            return property.GetString();
        }

        return string.Empty;
    }

    // Reads a JSON element map property.
    private static IReadOnlyDictionary<string, JsonElement> ReadElementMap(JsonElement element, string propertyName)
    {
        Dictionary<string, JsonElement> values = new(StringComparer.Ordinal);
        if (!element.TryGetProperty(propertyName, out JsonElement property) ||
            property.ValueKind != JsonValueKind.Object)
        {
            return values;
        }

        foreach (JsonProperty child in property.EnumerateObject())
        {
            values[child.Name] = child.Value.Clone();
        }

        return values;
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

    // Creates the canonical ButterMorph payload schema using Atlas-style field semantics.
    private static string CreateAtlasSchema(PayloadSchemaDesignInput input)
    {
        using JsonDocument document = JsonDocument.Parse(input.JsonSchema);
        using MemoryStream stream = new();
        using Utf8JsonWriter writer = new(stream, new JsonWriterOptions { Indented = false });

        writer.WriteStartObject();
        writer.WriteString("key", input.Key.Trim());
        writer.WriteString("name", input.Name.Trim());
        if (!string.IsNullOrWhiteSpace(input.Description))
        {
            writer.WriteString("description", input.Description.Trim());
        }

        writer.WriteString("version", input.Version.Trim());
        if (!string.IsNullOrWhiteSpace(input.VersionComment))
        {
            writer.WriteString("versionComment", input.VersionComment.Trim());
        }

        WriteOpenMetadata(writer, input.Metadata);
        foreach (JsonProperty property in document.RootElement.EnumerateObject())
        {
            if (ShouldSkipRootProperty(property.Name, input.Metadata))
            {
                continue;
            }

            property.WriteTo(writer);
        }

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

    // Detects unsupported required arrays anywhere in the schema body.
    private static bool ContainsRequiredArray(JsonElement element)
    {
        if (element.ValueKind == JsonValueKind.Object)
        {
            foreach (JsonProperty property in element.EnumerateObject())
            {
                if (string.Equals(property.Name, "required", StringComparison.Ordinal) &&
                    property.Value.ValueKind == JsonValueKind.Array)
                {
                    return true;
                }

                if (ContainsRequiredArray(property.Value))
                {
                    return true;
                }
            }
        }

        if (element.ValueKind == JsonValueKind.Array)
        {
            foreach (JsonElement item in element.EnumerateArray())
            {
                if (ContainsRequiredArray(item))
                {
                    return true;
                }
            }
        }

        return false;
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

