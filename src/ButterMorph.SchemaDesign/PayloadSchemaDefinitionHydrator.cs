namespace ButterMorph.SchemaDesign;

using System.Text.Json;

/// <summary>
/// Hydrates editable payload schema input from saved payload schema definitions.
/// </summary>
public sealed class PayloadSchemaDefinitionHydrator : IPayloadSchemaDefinitionHydrator
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    /// <inheritdoc />
    public PayloadSchemaDesignInput Hydrate(PayloadSchemaDefinition definition)
    {
        return new PayloadSchemaDesignInput
        {
            Key = definition.Key,
            Name = definition.Name,
            Description = definition.Description,
            Version = string.IsNullOrWhiteSpace(definition.Version) ? "1.0.0" : definition.Version,
            VersionComment = definition.VersionComment,
            Metadata = ReadMetadata(definition.Metadata),
            JsonSchema = JsonSerializer.Serialize(definition, JsonOptions)
        };
    }

    private static IReadOnlyDictionary<string, string> ReadMetadata(IReadOnlyDictionary<string, JsonElement> metadata)
    {
        Dictionary<string, string> values = new(StringComparer.Ordinal);
        if (metadata == null)
        {
            return values;
        }

        foreach (KeyValuePair<string, JsonElement> item in metadata)
        {
            values[item.Key] = item.Value.ValueKind == JsonValueKind.String
                ? item.Value.GetString()
                : item.Value.GetRawText();
        }

        return values;
    }
}
