namespace ButterMorph.SchemaDesign;

using System.Globalization;
using System.Text;
using System.Text.Json;

/// <summary>
/// Builds payload schemas from structured field definitions.
/// </summary>
public sealed class PayloadSchemaDefinitionBuilder : IPayloadSchemaDefinitionBuilder
{
    // JSON text used for map-shaped schemas.
    private const string MapType = "obj" + "ect";

    private readonly IPayloadSchemaBuilder payloadSchemaBuilder;

    /// <summary>
    /// Initializes a new instance of the <see cref="PayloadSchemaDefinitionBuilder"/> class.
    /// </summary>
    /// <param name="payloadSchemaBuilder">The payload schema builder.</param>
    public PayloadSchemaDefinitionBuilder(IPayloadSchemaBuilder payloadSchemaBuilder)
    {
        this.payloadSchemaBuilder = payloadSchemaBuilder;
    }

    /// <inheritdoc />
    public PayloadSchemaDesignResult Build(PayloadSchemaDesignInput input, IReadOnlyCollection<PayloadSchemaField> fields, IReadOnlyCollection<SchemaTypeCatalogItem> schemaTypes, IReadOnlyCollection<FieldMetadataCatalogItem> metadataFields)
    {
        input.JsonSchema = CreateBody(fields, schemaTypes);
        return payloadSchemaBuilder.Build(input, schemaTypes, metadataFields);
    }

    // Creates the schema body consumed by PayloadSchemaBuilder.
    private static string CreateBody(IReadOnlyCollection<PayloadSchemaField> fields, IReadOnlyCollection<SchemaTypeCatalogItem> schemaTypes)
    {
        Dictionary<string, JsonElement> definitions = new(StringComparer.Ordinal);
        using MemoryStream stream = new();
        using Utf8JsonWriter writer = new(stream, new JsonWriterOptions { Indented = false });
        writer.WriteStartObject();
        writer.WriteString("type", MapType);
        WriteProperties(writer, fields, schemaTypes, definitions);
        if (definitions.Count > 0)
        {
            writer.WritePropertyName("$defs");
            writer.WriteStartObject();
            foreach (KeyValuePair<string, JsonElement> definition in definitions)
            {
                writer.WritePropertyName(definition.Key);
                WriteDefinitionBody(writer, definition.Value);
            }

            writer.WriteEndObject();
        }

        writer.WriteEndObject();
        writer.Flush();
        return Encoding.UTF8.GetString(stream.ToArray());
    }

    // Writes object properties.
    private static void WriteProperties(Utf8JsonWriter writer, IReadOnlyCollection<PayloadSchemaField> fields, IReadOnlyCollection<SchemaTypeCatalogItem> schemaTypes, Dictionary<string, JsonElement> definitions)
    {
        writer.WritePropertyName("properties");
        writer.WriteStartObject();
        foreach (PayloadSchemaField field in fields)
        {
            if (string.IsNullOrWhiteSpace(field.Name))
            {
                continue;
            }

            writer.WritePropertyName(field.Name.Trim());
            WriteFieldDefinition(writer, field, schemaTypes, definitions);
        }

        writer.WriteEndObject();
    }

    // Writes one field definition.
    private static void WriteFieldDefinition(Utf8JsonWriter writer, PayloadSchemaField field, IReadOnlyCollection<SchemaTypeCatalogItem> schemaTypes, Dictionary<string, JsonElement> definitions)
    {
        writer.WriteStartObject();
        SchemaTypeCatalogItem customType = FindType(field.CustomTypeVersionId, schemaTypes);
        if (!string.IsNullOrWhiteSpace(customType.TypeVersionId))
        {
            string definitionKey = ResolveDefinitionKey(customType);
            writer.WriteString("$ref", "#/$defs/" + definitionKey);
            AddDefinition(definitionKey, customType, definitions);
        }
        else
        {
            writer.WriteString("type", ResolveDataType(field.DataType));
        }

        if (!string.IsNullOrWhiteSpace(field.Description))
        {
            writer.WriteString("description", field.Description.Trim());
        }

        if (field.IsRequired)
        {
            writer.WriteBoolean("required", true);
        }

        WriteMetadata(writer, field.Metadata);
        WriteValidation(writer, field.Validation);
        if (string.Equals(ResolveDataType(field.DataType), MapType, StringComparison.OrdinalIgnoreCase) && string.IsNullOrWhiteSpace(customType.TypeVersionId))
        {
            WriteProperties(writer, field.Children, schemaTypes, definitions);
        }

        if (string.Equals(ResolveDataType(field.DataType), "array", StringComparison.OrdinalIgnoreCase) && string.IsNullOrWhiteSpace(customType.TypeVersionId))
        {
            writer.WritePropertyName("items");
            WriteFieldDefinition(writer, field.ArrayItem ?? new PayloadSchemaField { DataType = "string" }, schemaTypes, definitions);
        }

        writer.WriteEndObject();
    }

    // Writes metadata values.
    private static void WriteMetadata(Utf8JsonWriter writer, IReadOnlyDictionary<string, string> metadata)
    {
        if (metadata == null || metadata.Count == 0)
        {
            return;
        }

        writer.WritePropertyName("metadata");
        writer.WriteStartObject();
        foreach (KeyValuePair<string, string> pair in metadata)
        {
            writer.WriteString(pair.Key, pair.Value);
        }

        writer.WriteEndObject();
    }

    // Writes validation keywords.
    private static void WriteValidation(Utf8JsonWriter writer, IReadOnlyDictionary<string, string> validation)
    {
        if (validation == null)
        {
            return;
        }

        foreach (KeyValuePair<string, string> pair in validation)
        {
            if (int.TryParse(pair.Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out int integerValue))
            {
                writer.WriteNumber(pair.Key, integerValue);
                continue;
            }

            if (decimal.TryParse(pair.Value, NumberStyles.Number, CultureInfo.InvariantCulture, out decimal decimalValue))
            {
                writer.WriteNumber(pair.Key, decimalValue);
                continue;
            }

            if (TryWriteJsonValue(writer, pair.Key, pair.Value))
            {
                continue;
            }

            writer.WriteString(pair.Key, pair.Value);
        }
    }

    // Attempts to write one validation value as JSON.
    private static bool TryWriteJsonValue(Utf8JsonWriter writer, string key, string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        try
        {
            using JsonDocument document = JsonDocument.Parse(value);
            writer.WritePropertyName(key);
            document.RootElement.WriteTo(writer);
            return true;
        }
        catch (JsonException)
        {
            return false;
        }
    }

    // Adds one referenced type definition.
    private static void AddDefinition(string definitionKey, SchemaTypeCatalogItem customType, Dictionary<string, JsonElement> definitions)
    {
        if (definitions.ContainsKey(definitionKey) || string.IsNullOrWhiteSpace(customType.JsonSchema))
        {
            return;
        }

        using JsonDocument document = JsonDocument.Parse(customType.JsonSchema);
        definitions[definitionKey] = document.RootElement.Clone();
        AddNestedDefinitions(definitions, document.RootElement);
    }

    // Adds nested definitions from one custom type definition.
    private static void AddNestedDefinitions(Dictionary<string, JsonElement> definitions, JsonElement schema)
    {
        if (!schema.TryGetProperty("$defs", out JsonElement defs) || defs.ValueKind != JsonValueKind.Object)
        {
            return;
        }

        foreach (JsonProperty definition in defs.EnumerateObject())
        {
            if (!definitions.ContainsKey(definition.Name))
            {
                definitions[definition.Name] = definition.Value.Clone();
            }
        }
    }

    // Writes a definition without nested $defs.
    private static void WriteDefinitionBody(Utf8JsonWriter writer, JsonElement definition)
    {
        writer.WriteStartObject();
        foreach (JsonProperty property in definition.EnumerateObject())
        {
            if (string.Equals(property.Name, "$defs", StringComparison.Ordinal))
            {
                continue;
            }

            property.WriteTo(writer);
        }

        writer.WriteEndObject();
    }

    // Finds a custom type by version id.
    private static SchemaTypeCatalogItem FindType(string typeVersionId, IReadOnlyCollection<SchemaTypeCatalogItem> schemaTypes)
    {
        if (string.IsNullOrWhiteSpace(typeVersionId))
        {
            return new SchemaTypeCatalogItem();
        }

        foreach (SchemaTypeCatalogItem item in schemaTypes)
        {
            if (string.Equals(item.TypeVersionId, typeVersionId, StringComparison.OrdinalIgnoreCase))
            {
                return item;
            }
        }

        return new SchemaTypeCatalogItem();
    }

    // Resolves one data type into schema text.
    private static string ResolveDataType(string dataType)
    {
        if (string.Equals(dataType, MapType, StringComparison.OrdinalIgnoreCase))
        {
            return MapType;
        }

        if (string.Equals(dataType, "object", StringComparison.OrdinalIgnoreCase))
        {
            return MapType;
        }

        return string.IsNullOrWhiteSpace(dataType) ? "string" : dataType.Trim();
    }

    // Resolves a ButterMorph definition key.
    private static string ResolveDefinitionKey(SchemaTypeCatalogItem customType)
    {
        string key = string.IsNullOrWhiteSpace(customType.TypeId)
            ? customType.Name
            : customType.TypeId;

        return key.Trim() + "@" + customType.VersionNumber.Trim();
    }
}
