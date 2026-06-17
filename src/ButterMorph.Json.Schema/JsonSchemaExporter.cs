namespace ButterMorph.Json.Schema;

using System.Text;
using System.Text.Json;
using ButterMorph.Abstractions;

/// <summary>
/// Exports ButterMorph structure schemas to JSON Schema text.
/// </summary>
public sealed class JsonSchemaExporter : IJsonSchemaExporter
{
    // Stores metadata keys that can be emitted as JSON Schema keywords.
    private static readonly HashSet<string> KeywordMetadata = new(StringComparer.Ordinal)
    {
        "title",
        "description",
        "format",
        "pattern",
        "minLength",
        "maxLength",
        "minimum",
        "maximum",
        "precision",
        "scale",
        "minItems",
        "maxItems",
        "enum",
        "typeId",
        "typeVersionId",
        "$ref"
    };

    /// <summary>
    /// Exports a ButterMorph schema.
    /// </summary>
    /// <param name="request">The export request.</param>
    /// <returns>The conversion result.</returns>
    public JsonSchemaConversionResult Export(JsonSchemaExportRequest request)
    {
        using MemoryStream stream = new();
        using Utf8JsonWriter writer = new(stream, new JsonWriterOptions
        {
            Indented = false
        });

        WriteNode(writer, request.Schema.Root, true);
        writer.Flush();

        string jsonSchema = Encoding.UTF8.GetString(stream.ToArray());

        return new JsonSchemaConversionResult
        {
            Succeeded = true,
            Schema = request.Schema,
            JsonSchema = jsonSchema,
            Diagnostics = []
        };
    }

    // Writes one schema node as JSON Schema.
    private static void WriteNode(Utf8JsonWriter writer, ISchemaNode node, bool includeSchemaMetadata)
    {
        writer.WriteStartObject();

        if (node.Kind == SchemaNodeKind.Object)
        {
            writer.WriteString("type", SchemaText.Map);
            WriteMetadata(writer, node.Metadata);
            WriteProperties(writer, node);
            writer.WriteEndObject();
            return;
        }

        if (node.Kind == SchemaNodeKind.Array)
        {
            writer.WriteString("type", SchemaText.Array);
            WriteMetadata(writer, node.Metadata);
            WriteItems(writer, node);
            writer.WriteEndObject();
            return;
        }

        writer.WriteString("type", NormalizeScalarType(node.DataType));
        WriteMetadata(writer, node.Metadata);
        writer.WriteEndObject();
    }

    // Writes child schema nodes as properties.
    private static void WriteProperties(Utf8JsonWriter writer, ISchemaNode node)
    {
        writer.WritePropertyName("properties");
        writer.WriteStartObject();

        foreach (ISchemaNode child in node.Children)
        {
            writer.WritePropertyName(child.Name);
            WriteNode(writer, child, false);
        }

        writer.WriteEndObject();
        WriteRequired(writer, node.Children);
    }

    // Writes array item schema.
    private static void WriteItems(Utf8JsonWriter writer, ISchemaNode node)
    {
        ISchemaNode item = new ButterMorph.Core.SchemaNode
        {
            Name = "$item",
            Kind = SchemaNodeKind.Scalar,
            DataType = SchemaText.String,
            Children = []
        };

        foreach (ISchemaNode child in node.Children)
        {
            item = child;
            break;
        }

        writer.WritePropertyName("items");
        WriteNode(writer, item, false);
    }

    // Writes standard required names.
    private static void WriteRequired(Utf8JsonWriter writer, IReadOnlyCollection<ISchemaNode> children)
    {
        List<string> requiredNames = [];

        foreach (ISchemaNode child in children)
        {
            if (child.IsRequired)
            {
                requiredNames.Add(child.Name);
            }
        }

        if (requiredNames.Count == 0)
        {
            return;
        }

        writer.WritePropertyName("required");
        writer.WriteStartArray();

        foreach (string name in requiredNames)
        {
            writer.WriteStringValue(name);
        }

        writer.WriteEndArray();
    }

    // Writes metadata as JSON Schema keywords.
    private static void WriteMetadata(Utf8JsonWriter writer, IReadOnlyDictionary<string, string> metadata)
    {
        foreach (KeyValuePair<string, string> pair in metadata)
        {
            if (string.Equals(pair.Key, "json:$defs", StringComparison.Ordinal))
            {
                WriteRawProperty(writer, "$defs", pair.Value);
                continue;
            }

            if (pair.Key.StartsWith("json:", StringComparison.Ordinal))
            {
                WriteRawProperty(writer, pair.Key["json:".Length..], pair.Value);
                continue;
            }

            if (!KeywordMetadata.Contains(pair.Key))
            {
                continue;
            }

            WriteKeywordMetadata(writer, pair.Key, pair.Value);
        }
    }

    // Writes one metadata keyword using numeric, boolean, JSON, or string shape.
    private static void WriteKeywordMetadata(Utf8JsonWriter writer, string key, string value)
    {
        if (IsRawJsonKeyword(key))
        {
            WriteRawProperty(writer, key, value);
            return;
        }

        if (decimal.TryParse(value, System.Globalization.NumberStyles.Number, System.Globalization.CultureInfo.InvariantCulture, out decimal decimalValue))
        {
            writer.WriteNumber(key, decimalValue);
            return;
        }

        if (string.Equals(value, "true", StringComparison.OrdinalIgnoreCase))
        {
            writer.WriteBoolean(key, true);
            return;
        }

        if (string.Equals(value, "false", StringComparison.OrdinalIgnoreCase))
        {
            writer.WriteBoolean(key, false);
            return;
        }

        writer.WriteString(key, value);
    }

    // Writes a raw JSON property with string fallback.
    private static void WriteRawProperty(Utf8JsonWriter writer, string key, string rawJson)
    {
        writer.WritePropertyName(key);

        try
        {
            using JsonDocument document = JsonDocument.Parse(rawJson);
            document.RootElement.WriteTo(writer);
        }
        catch (JsonException)
        {
            writer.WriteStringValue(rawJson);
        }
    }

    // Determines whether a keyword should be emitted as raw JSON.
    private static bool IsRawJsonKeyword(string key)
    {
        return string.Equals(key, "enum", StringComparison.Ordinal);
    }

    // Normalizes scalar type names for JSON Schema.
    private static string NormalizeScalarType(string dataType)
    {
        if (string.IsNullOrWhiteSpace(dataType))
        {
            return SchemaText.String;
        }

        return dataType;
    }
}
