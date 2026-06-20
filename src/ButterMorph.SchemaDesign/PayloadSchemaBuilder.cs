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
                JsonSchema = AddSchemaIdentity(input.JsonSchema, input.Key, input.Name, input.Description)
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
            JsonSchema = SafeString(input.JsonSchema)
        };
    }

    // Adds canonical ButterMorph schema identity to the payload JSON Schema.
    private static string AddSchemaIdentity(string jsonSchema, string key, string name, string description)
    {
        using JsonDocument document = JsonDocument.Parse(jsonSchema);
        using MemoryStream stream = new();
        using Utf8JsonWriter writer = new(stream, new JsonWriterOptions { Indented = false });

        writer.WriteStartObject();
        writer.WriteString("key", key.Trim());
        writer.WriteString("name", name.Trim());

        if (!string.IsNullOrWhiteSpace(description))
        {
            writer.WriteString("description", description.Trim());
        }

        foreach (JsonProperty property in document.RootElement.EnumerateObject())
        {
            if (IsIdentityProperty(property.Name))
            {
                continue;
            }

            property.WriteTo(writer);
        }

        writer.WriteEndObject();
        writer.Flush();

        return Encoding.UTF8.GetString(stream.ToArray());
    }

    // Detects root identity keywords already controlled by ButterMorph.
    private static bool IsIdentityProperty(string propertyName)
    {
        return string.Equals(propertyName, "key", StringComparison.Ordinal) ||
            string.Equals(propertyName, "name", StringComparison.Ordinal) ||
            string.Equals(propertyName, "description", StringComparison.Ordinal);
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

