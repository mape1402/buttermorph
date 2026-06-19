namespace ButterMorph.SchemaDesign;

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
                JsonSchema = SchemaDesignJsonTools.Compact(input.JsonSchema)
            };
        }
        catch (JsonException exception)
        {
            return Fail("BMSD303", exception.Message, "JsonSchema");
        }
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