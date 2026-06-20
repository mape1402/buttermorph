namespace ButterMorph.Json.Schema;

using System.Text.Json;
using ButterMorph.Abstractions;
using ButterMorph.Core;

/// <summary>
/// Imports JSON Schema text into ButterMorph structure schemas.
/// </summary>
public sealed class JsonSchemaImporter : IJsonSchemaImporter
{
    // Stores JSON Schema keywords that are handled directly by the importer.
    private static readonly HashSet<string> KnownKeywords = new(StringComparer.Ordinal)
    {
        "type",
        "properties",
        "items",
        "required",
        "$defs",
        "key",
        "name",
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
    /// Imports a JSON Schema document.
    /// </summary>
    /// <param name="request">The import request.</param>
    /// <returns>The conversion result.</returns>
    public JsonSchemaConversionResult Import(JsonSchemaImportRequest request)
    {
        try
        {
            using JsonDocument document = JsonDocument.Parse(request.JsonSchema);
            JsonElement rootElement = document.RootElement;
            string schemaKey = ResolveSchemaKey(request.Name, rootElement);
            if (string.IsNullOrWhiteSpace(schemaKey))
            {
                return CreateFailure("Schema key is required.");
            }

            string schemaName = ResolveSchemaName(schemaKey, rootElement);
            Dictionary<string, string> schemaMetadata = ReadMetadata(rootElement);

            if (rootElement.TryGetProperty("$defs", out JsonElement definitions))
            {
                schemaMetadata["json:$defs"] = definitions.GetRawText();
            }

            return new JsonSchemaConversionResult
            {
                Succeeded = true,
                Schema = new StructureSchema
                {
                    Key = schemaKey,
                    Name = schemaName,
                    Description = ResolveSchemaDescription(rootElement),
                    Root = ImportNode("$root", rootElement, ReadRequiredNames(rootElement)),
                    Metadata = schemaMetadata
                },
                JsonSchema = request.JsonSchema,
                Diagnostics = []
            };
        }
        catch (JsonException exception)
        {
            return CreateFailure(exception.Message);
        }
    }

    // Imports one JSON Schema element as a ButterMorph schema node.
    private static ISchemaNode ImportNode(string name, JsonElement element, HashSet<string> requiredNames)
    {
        string type = ReadType(element);
        bool isRequired = requiredNames.Contains(name) || ReadAtlasRequired(element);
        Dictionary<string, string> metadata = ReadMetadata(element);

        if (IsMapType(type))
        {
            List<ISchemaNode> children = [];
            HashSet<string> childRequiredNames = ReadRequiredNames(element);

            if (element.TryGetProperty("properties", out JsonElement properties) && properties.ValueKind == JsonValueKind.Object)
            {
                foreach (JsonProperty property in properties.EnumerateObject())
                {
                    children.Add(ImportNode(property.Name, property.Value, childRequiredNames));
                }
            }

            return new SchemaNode
            {
                Name = name,
                Kind = SchemaNodeKind.Object,
                DataType = SchemaText.Map,
                IsRequired = isRequired,
                Children = children,
                Metadata = metadata
            };
        }

        if (string.Equals(type, SchemaText.Array, StringComparison.OrdinalIgnoreCase))
        {
            List<ISchemaNode> children = [];

            if (element.TryGetProperty("items", out JsonElement items))
            {
                children.Add(ImportNode("$item", items, []));
            }

            return new SchemaNode
            {
                Name = name,
                Kind = SchemaNodeKind.Array,
                DataType = SchemaText.Array,
                IsRequired = isRequired,
                Children = children,
                Metadata = metadata
            };
        }

        return new SchemaNode
        {
            Name = name,
            Kind = SchemaNodeKind.Scalar,
            DataType = type,
            IsRequired = isRequired,
            Children = [],
            Metadata = metadata
        };
    }

    // Reads metadata and unknown keywords from a JSON Schema element.
    private static Dictionary<string, string> ReadMetadata(JsonElement element)
    {
        Dictionary<string, string> metadata = new(StringComparer.Ordinal);

        if (element.ValueKind != JsonValueKind.Object)
        {
            return metadata;
        }

        foreach (JsonProperty property in element.EnumerateObject())
        {
            if (string.Equals(property.Name, "properties", StringComparison.Ordinal) ||
                string.Equals(property.Name, "items", StringComparison.Ordinal) ||
                string.Equals(property.Name, "required", StringComparison.Ordinal) ||
                string.Equals(property.Name, "type", StringComparison.Ordinal))
            {
                continue;
            }

            if (KnownKeywords.Contains(property.Name))
            {
                metadata[property.Name] = ReadMetadataValue(property.Value);
                continue;
            }

            metadata["json:" + property.Name] = property.Value.GetRawText();
        }

        return metadata;
    }

    // Reads a metadata value as compact text.
    private static string ReadMetadataValue(JsonElement element)
    {
        if (element.ValueKind == JsonValueKind.String)
        {
            string value = element.GetString();
            return value;
        }

        if (element.ValueKind == JsonValueKind.True)
        {
            return "true";
        }

        if (element.ValueKind == JsonValueKind.False)
        {
            return "false";
        }

        return element.GetRawText();
    }

    // Reads schema type text with a scalar fallback.
    private static string ReadType(JsonElement element)
    {
        if (element.TryGetProperty("type", out JsonElement typeElement) && typeElement.ValueKind == JsonValueKind.String)
        {
            string value = typeElement.GetString();

            if (!string.IsNullOrWhiteSpace(value))
            {
                return value;
            }
        }

        if (element.TryGetProperty("$ref", out JsonElement referenceElement) && referenceElement.ValueKind == JsonValueKind.String)
        {
            return SchemaText.Map;
        }

        return SchemaText.String;
    }

    // Reads standard JSON Schema required names.
    private static HashSet<string> ReadRequiredNames(JsonElement element)
    {
        HashSet<string> names = new(StringComparer.Ordinal);

        if (!element.TryGetProperty("required", out JsonElement requiredElement) || requiredElement.ValueKind != JsonValueKind.Array)
        {
            return names;
        }

        foreach (JsonElement item in requiredElement.EnumerateArray())
        {
            if (item.ValueKind != JsonValueKind.String)
            {
                continue;
            }

            string value = item.GetString();

            if (!string.IsNullOrWhiteSpace(value))
            {
                names.Add(value);
            }
        }

        return names;
    }

    // Reads the Atlas per-property required flag.
    private static bool ReadAtlasRequired(JsonElement element)
    {
        if (!element.TryGetProperty("required", out JsonElement requiredElement))
        {
            return false;
        }

        if (requiredElement.ValueKind == JsonValueKind.True)
        {
            return true;
        }

        return false;
    }

    // Resolves the ButterMorph schema key.
    private static string ResolveSchemaKey(string requestedName, JsonElement rootElement)
    {
        if (rootElement.TryGetProperty("key", out JsonElement keyElement) && keyElement.ValueKind == JsonValueKind.String)
        {
            string key = keyElement.GetString();

            if (!string.IsNullOrWhiteSpace(key))
            {
                return key;
            }
        }

        if (!string.IsNullOrWhiteSpace(requestedName))
        {
            return requestedName;
        }

        return string.Empty;
    }

    // Resolves the ButterMorph schema name.
    private static string ResolveSchemaName(string schemaKey, JsonElement rootElement)
    {
        if (rootElement.TryGetProperty("name", out JsonElement nameElement) && nameElement.ValueKind == JsonValueKind.String)
        {
            string name = nameElement.GetString();

            if (!string.IsNullOrWhiteSpace(name))
            {
                return name;
            }
        }

        return schemaKey;
    }

    // Resolves the ButterMorph schema description.
    private static string ResolveSchemaDescription(JsonElement rootElement)
    {
        if (rootElement.TryGetProperty("description", out JsonElement descriptionElement) && descriptionElement.ValueKind == JsonValueKind.String)
        {
            string description = descriptionElement.GetString();

            if (!string.IsNullOrWhiteSpace(description))
            {
                return description;
            }
        }

        return string.Empty;
    }

    // Determines whether a schema type represents a map-shaped node.
    private static bool IsMapType(string type)
    {
        return string.Equals(type, SchemaText.Map, StringComparison.OrdinalIgnoreCase);
    }

    // Creates a failed conversion result.
    private static JsonSchemaConversionResult CreateFailure(string message)
    {
        return new JsonSchemaConversionResult
        {
            Succeeded = false,
            Diagnostics =
            [
                new DiagnosticEntry
                {
                    Code = "BMJS001",
                    Message = message,
                    Path = string.Empty,
                    Severity = "Error"
                }
            ]
        };
    }
}

