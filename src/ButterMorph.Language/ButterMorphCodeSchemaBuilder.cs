namespace ButterMorph.Language;

using System.Text.Json;
using ButterMorph.SchemaDesign;

/// <summary>
/// Builds a neutral schema model for language adapters.
/// </summary>
public static class ButterMorphCodeSchemaBuilder
{
    private const string MapType = "object";

    /// <summary>
    /// Builds a language-adapter schema model from a payload schema definition and raw schema JSON.
    /// </summary>
    /// <param name="definition">The schema definition when available.</param>
    /// <param name="rawJson">The raw JSON schema.</param>
    /// <returns>The schema model.</returns>
    public static ButterMorphCodeSchemaModel Build(PayloadSchemaDefinition definition, string rawJson)
    {
        string json = string.IsNullOrWhiteSpace(rawJson) ? "{}" : rawJson;
        try
        {
            using JsonDocument document = JsonDocument.Parse(json);
            return Build(definition, document.RootElement);
        }
        catch (JsonException)
        {
            string name = ResolveSchemaName(definition, default);
            return new ButterMorphCodeSchemaModel
            {
                Key = definition?.Key ?? string.Empty,
                Name = name,
                Description = definition?.Description ?? string.Empty,
                Root = new ButterMorphCodeSchemaNode
                {
                    Name = name,
                    Kind = MapType,
                    DataType = MapType,
                    Description = definition?.Description ?? string.Empty
                }
            };
        }
    }

    private static ButterMorphCodeSchemaModel Build(PayloadSchemaDefinition definition, JsonElement root)
    {
        IReadOnlyCollection<ButterMorphCodeSchemaNode> children = TryGetObject(root, "properties", out JsonElement properties)
            ? ReadProperties(properties, string.Empty, root, [])
            : [];
        string name = ResolveSchemaName(definition, root);
        string description = ResolveValue(definition?.Description, ReadString(root, "description", string.Empty));

        return new ButterMorphCodeSchemaModel
        {
            Key = ResolveValue(definition?.Key, ReadString(root, "key", string.Empty)),
            Name = name,
            Description = description,
            Root = new ButterMorphCodeSchemaNode
            {
                Name = name,
                Path = string.Empty,
                DataType = MapType,
                Kind = MapType,
                Description = description,
                Children = children
            }
        };
    }

    private static IReadOnlyCollection<ButterMorphCodeSchemaNode> ReadProperties(
        JsonElement properties,
        string parentPath,
        JsonElement root,
        HashSet<string> visitedRefs)
    {
        List<ButterMorphCodeSchemaNode> nodes = [];
        foreach (JsonProperty property in properties.EnumerateObject())
        {
            nodes.Add(ReadField(property.Name, property.Value, parentPath, root, visitedRefs));
        }

        return nodes;
    }

    private static ButterMorphCodeSchemaNode ReadField(
        string name,
        JsonElement element,
        string parentPath,
        JsonElement root,
        HashSet<string> visitedRefs)
    {
        string path = string.IsNullOrWhiteSpace(parentPath) ? name : parentPath + "." + name;
        string reference = ReadString(element, "$ref", string.Empty);
        JsonElement effectiveElement = ResolveEffectiveElement(element, root, reference);
        IReadOnlyCollection<ButterMorphCodeSchemaNode> children = ReadChildren(element, effectiveElement, path, root, visitedRefs);

        return new ButterMorphCodeSchemaNode
        {
            Name = name,
            Path = path,
            DataType = ResolveDisplayType(element, effectiveElement, root),
            Kind = ResolveKind(element, effectiveElement),
            Description = ResolveValue(ReadString(element, "description", string.Empty), ReadString(effectiveElement, "description", string.Empty)),
            Reference = reference,
            ItemReference = ReadArrayItemReference(element),
            IsRequired = ReadBoolean(element, "required"),
            Children = children
        };
    }

    private static IReadOnlyCollection<ButterMorphCodeSchemaNode> ReadChildren(
        JsonElement element,
        JsonElement effectiveElement,
        string path,
        JsonElement root,
        HashSet<string> visitedRefs)
    {
        if (TryGetObject(element, "items", out JsonElement items))
        {
            string itemPath = path + "[]";
            JsonElement effectiveItems = ResolveEffectiveElement(items, root, ReadString(items, "$ref", string.Empty));
            if (TryGetObject(effectiveItems, "properties", out JsonElement itemProperties))
            {
                return ReadProperties(itemProperties, itemPath, root, visitedRefs);
            }

            if (TryGetObject(items, "properties", out JsonElement inlineItemProperties))
            {
                return ReadProperties(inlineItemProperties, itemPath, root, visitedRefs);
            }
        }

        if (TryGetObject(element, "properties", out JsonElement childProperties))
        {
            return ReadProperties(childProperties, path, root, visitedRefs);
        }

        if (TryGetObject(effectiveElement, "properties", out JsonElement effectiveProperties))
        {
            string reference = ReadString(element, "$ref", string.Empty);
            if (string.IsNullOrWhiteSpace(reference) || visitedRefs.Add(reference))
            {
                IReadOnlyCollection<ButterMorphCodeSchemaNode> nodes = ReadProperties(effectiveProperties, path, root, visitedRefs);
                if (!string.IsNullOrWhiteSpace(reference))
                {
                    visitedRefs.Remove(reference);
                }

                return nodes;
            }
        }

        return [];
    }

    private static JsonElement ResolveEffectiveElement(JsonElement element, JsonElement root, string reference)
    {
        return !string.IsNullOrWhiteSpace(reference) && TryResolveReference(root, reference, out JsonElement definition)
            ? definition
            : element;
    }

    private static string ReadArrayItemReference(JsonElement element)
    {
        return TryGetObject(element, "items", out JsonElement items)
            ? ReadString(items, "$ref", string.Empty)
            : string.Empty;
    }

    private static string ResolveDisplayType(JsonElement element, JsonElement effectiveElement, JsonElement root)
    {
        if (!string.IsNullOrWhiteSpace(ReadString(element, "$ref", string.Empty)))
        {
            return "custom type";
        }

        string type = NormalizeType(ReadString(element, "type", ReadString(effectiveElement, "type", string.Empty)));
        if (string.Equals(type, "array", StringComparison.OrdinalIgnoreCase) &&
            TryGetObject(element, "items", out JsonElement items))
        {
            return "array of " + ResolveDisplayType(items, ResolveEffectiveElement(items, root, ReadString(items, "$ref", string.Empty)), root);
        }

        return string.IsNullOrWhiteSpace(type) ? ResolveKind(element, effectiveElement) : type;
    }

    private static string ResolveKind(JsonElement element, JsonElement effectiveElement)
    {
        string type = NormalizeType(ReadString(element, "type", ReadString(effectiveElement, "type", string.Empty)));
        if (string.Equals(type, "array", StringComparison.OrdinalIgnoreCase))
        {
            return "array";
        }

        if (string.Equals(type, MapType, StringComparison.OrdinalIgnoreCase) ||
            TryGetObject(element, "properties", out _) ||
            TryGetObject(effectiveElement, "properties", out _))
        {
            return MapType;
        }

        if (!string.IsNullOrWhiteSpace(ReadString(element, "$ref", string.Empty)))
        {
            return "custom";
        }

        return "scalar";
    }

    private static bool TryResolveReference(JsonElement root, string reference, out JsonElement definition)
    {
        definition = default;
        string marker = "#/$defs/";
        if (!reference.StartsWith(marker, StringComparison.Ordinal))
        {
            return false;
        }

        string key = reference[marker.Length..];
        return TryGetObject(root, "$defs", out JsonElement definitions) &&
            definitions.TryGetProperty(key, out definition) &&
            definition.ValueKind == JsonValueKind.Object;
    }

    private static string ResolveSchemaName(PayloadSchemaDefinition definition, JsonElement root)
    {
        return ResolveValue(definition?.Name, ReadString(root, "name", string.Empty), definition?.Key, "Schema");
    }

    private static string ResolveValue(params string[] values)
    {
        foreach (string value in values)
        {
            if (!string.IsNullOrWhiteSpace(value))
            {
                return value;
            }
        }

        return string.Empty;
    }

    private static string ReadString(JsonElement element, string propertyName, string fallback)
    {
        if (element.ValueKind == JsonValueKind.Object &&
            element.TryGetProperty(propertyName, out JsonElement property) &&
            property.ValueKind == JsonValueKind.String)
        {
            return property.GetString() ?? fallback;
        }

        return fallback;
    }

    private static bool ReadBoolean(JsonElement element, string propertyName)
    {
        return element.ValueKind == JsonValueKind.Object &&
            element.TryGetProperty(propertyName, out JsonElement property) &&
            property.ValueKind == JsonValueKind.True;
    }

    private static bool TryGetObject(JsonElement element, string propertyName, out JsonElement property)
    {
        property = default;
        return element.ValueKind == JsonValueKind.Object &&
            element.TryGetProperty(propertyName, out property) &&
            property.ValueKind == JsonValueKind.Object;
    }

    private static string NormalizeType(string type)
    {
        if (string.Equals(type, "object", StringComparison.OrdinalIgnoreCase))
        {
            return MapType;
        }

        return type;
    }
}
