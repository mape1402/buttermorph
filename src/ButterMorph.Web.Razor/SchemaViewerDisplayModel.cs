namespace ButterMorph.Web.Razor;

using System.Text.Json;
using ButterMorph.SchemaDesign;

internal sealed class SchemaViewerDisplayModel
{
    public string Key { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string Version { get; set; } = string.Empty;

    public string VersionComment { get; set; } = string.Empty;

    public IReadOnlyCollection<SchemaViewerEntryDisplayModel> Metadata { get; set; } = [];

    public SchemaViewerNodeDisplayModel Root { get; set; } = new();

    public IReadOnlyCollection<SchemaViewerNodeDisplayModel> Fields { get; set; } = [];
}

internal sealed class SchemaViewerNodeDisplayModel
{
    public string Id { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string Path { get; set; } = string.Empty;

    public string DataType { get; set; } = string.Empty;

    public string Kind { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string Reference { get; set; } = string.Empty;

    public bool IsRequired { get; set; }

    public IReadOnlyCollection<SchemaViewerEntryDisplayModel> Restrictions { get; set; } = [];

    public IReadOnlyCollection<SchemaViewerEntryDisplayModel> Metadata { get; set; } = [];

    public IReadOnlyCollection<string> Badges { get; set; } = [];

    public IReadOnlyCollection<SchemaViewerNodeDisplayModel> Children { get; set; } = [];
}

internal sealed class SchemaViewerEntryDisplayModel
{
    public string Key { get; set; } = string.Empty;

    public string Label { get; set; } = string.Empty;

    public string Value { get; set; } = string.Empty;
}

internal static class SchemaViewerDisplayBuilder
{
    private const string MapType = "object";

    private static readonly IReadOnlyDictionary<string, string> RestrictionLabels = new Dictionary<string, string>(StringComparer.Ordinal)
    {
        ["minLength"] = "Min length",
        ["maxLength"] = "Max length",
        ["minimum"] = "Minimum",
        ["maximum"] = "Maximum",
        ["exclusiveMinimum"] = "Exclusive minimum",
        ["exclusiveMaximum"] = "Exclusive maximum",
        ["minItems"] = "Min items",
        ["maxItems"] = "Max items",
        ["pattern"] = "Pattern",
        ["format"] = "Format",
        ["enum"] = "Allowed values",
        ["allowedValues"] = "Allowed values"
    };

    internal static SchemaViewerDisplayModel Build(PayloadSchemaDefinition definition, string rawJson)
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
            return new SchemaViewerDisplayModel
            {
                Key = definition?.Key ?? string.Empty,
                Name = name,
                Description = definition?.Description ?? string.Empty,
                Version = definition?.Version ?? string.Empty,
                VersionComment = definition?.VersionComment ?? string.Empty,
                Root = new SchemaViewerNodeDisplayModel
                {
                    Id = "root",
                    Name = name,
                    Kind = "object",
                    DataType = MapType
                }
            };
        }
    }

    private static SchemaViewerDisplayModel Build(PayloadSchemaDefinition definition, JsonElement root)
    {
        List<SchemaViewerNodeDisplayModel> fields = [];
        int nextId = 0;
        IReadOnlyCollection<SchemaViewerNodeDisplayModel> children = TryGetObject(root, "properties", out JsonElement properties)
            ? ReadProperties(properties, string.Empty, root, fields, [], ref nextId)
            : [];

        IReadOnlyCollection<SchemaViewerEntryDisplayModel> metadata = ReadMetadata(root);
        if (metadata.Count == 0 && definition?.Metadata != null)
        {
            metadata = definition.Metadata
                .Select(pair => new SchemaViewerEntryDisplayModel
                {
                    Key = pair.Key,
                    Label = pair.Key,
                    Value = ReadValue(pair.Value)
                })
                .ToArray();
        }

        string name = ResolveSchemaName(definition, root);
        string description = ResolveValue(definition?.Description, ReadString(root, "description", string.Empty));
        return new SchemaViewerDisplayModel
        {
            Key = ResolveValue(definition?.Key, ReadString(root, "key", string.Empty)),
            Name = name,
            Description = description,
            Version = ResolveValue(definition?.Version, ReadString(root, "version", string.Empty)),
            VersionComment = ResolveValue(definition?.VersionComment, ReadString(root, "versionComment", string.Empty)),
            Metadata = metadata,
            Root = new SchemaViewerNodeDisplayModel
            {
                Id = "root",
                Name = name,
                Path = string.Empty,
                DataType = MapType,
                Kind = "object",
                Description = description,
                Metadata = metadata,
                Children = children
            },
            Fields = fields
        };
    }

    private static IReadOnlyCollection<SchemaViewerNodeDisplayModel> ReadProperties(
        JsonElement properties,
        string parentPath,
        JsonElement root,
        List<SchemaViewerNodeDisplayModel> fields,
        HashSet<string> visitedRefs,
        ref int nextId)
    {
        List<SchemaViewerNodeDisplayModel> nodes = [];
        foreach (JsonProperty property in properties.EnumerateObject())
        {
            nodes.Add(ReadField(property.Name, property.Value, parentPath, root, fields, visitedRefs, ref nextId));
        }

        return nodes;
    }

    private static SchemaViewerNodeDisplayModel ReadField(
        string name,
        JsonElement element,
        string parentPath,
        JsonElement root,
        List<SchemaViewerNodeDisplayModel> fields,
        HashSet<string> visitedRefs,
        ref int nextId)
    {
        string path = string.IsNullOrWhiteSpace(parentPath) ? name : parentPath + "." + name;
        string reference = ReadString(element, "$ref", string.Empty);
        JsonElement effectiveElement = ResolveEffectiveElement(element, root, reference);
        string kind = ResolveKind(element, effectiveElement);
        string dataType = ResolveDisplayType(element, effectiveElement, root);
        IReadOnlyCollection<SchemaViewerEntryDisplayModel> restrictions = MergeEntries(ReadRestrictions(effectiveElement), ReadRestrictions(element));
        IReadOnlyCollection<SchemaViewerEntryDisplayModel> metadata = ReadMetadata(element);
        IReadOnlyCollection<SchemaViewerNodeDisplayModel> children = ReadChildren(element, effectiveElement, path, root, fields, visitedRefs, ref nextId);
        bool isRequired = ReadBoolean(element, "required");
        SchemaViewerNodeDisplayModel field = new()
        {
            Id = "field-" + nextId.ToString(System.Globalization.CultureInfo.InvariantCulture),
            Name = name,
            Path = path,
            DataType = dataType,
            Kind = kind,
            Description = ResolveValue(ReadString(element, "description", string.Empty), ReadString(effectiveElement, "description", string.Empty)),
            Reference = reference,
            IsRequired = isRequired,
            Restrictions = restrictions,
            Metadata = metadata,
            Badges = CreateBadges(kind, dataType, isRequired, restrictions.Count > 0, children.Count > 0),
            Children = children
        };

        nextId++;
        fields.Add(field);
        return field;
    }

    private static IReadOnlyCollection<SchemaViewerNodeDisplayModel> ReadChildren(
        JsonElement element,
        JsonElement effectiveElement,
        string path,
        JsonElement root,
        List<SchemaViewerNodeDisplayModel> fields,
        HashSet<string> visitedRefs,
        ref int nextId)
    {
        if (TryGetObject(element, "items", out JsonElement items))
        {
            string itemPath = path + "[]";
            JsonElement effectiveItems = ResolveEffectiveElement(items, root, ReadString(items, "$ref", string.Empty));
            if (TryGetObject(effectiveItems, "properties", out JsonElement itemProperties))
            {
                return ReadProperties(itemProperties, itemPath, root, fields, visitedRefs, ref nextId);
            }

            if (TryGetObject(items, "properties", out JsonElement inlineItemProperties))
            {
                return ReadProperties(inlineItemProperties, itemPath, root, fields, visitedRefs, ref nextId);
            }
        }

        if (TryGetObject(element, "properties", out JsonElement childProperties))
        {
            return ReadProperties(childProperties, path, root, fields, visitedRefs, ref nextId);
        }

        if (TryGetObject(effectiveElement, "properties", out JsonElement effectiveProperties))
        {
            string reference = ReadString(element, "$ref", string.Empty);
            if (string.IsNullOrWhiteSpace(reference) || visitedRefs.Add(reference))
            {
                IReadOnlyCollection<SchemaViewerNodeDisplayModel> nodes = ReadProperties(effectiveProperties, path, root, fields, visitedRefs, ref nextId);
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

    private static IReadOnlyCollection<string> CreateBadges(string kind, string dataType, bool isRequired, bool hasRestrictions, bool hasChildren)
    {
        List<string> badges =
        [
            isRequired ? "Required" : "Optional",
            dataType
        ];

        if (string.Equals(kind, "array", StringComparison.Ordinal))
        {
            badges.Add("Array");
        }
        else if (string.Equals(kind, "object", StringComparison.Ordinal))
        {
            badges.Add("Object");
        }
        else if (string.Equals(kind, "custom", StringComparison.Ordinal))
        {
            badges.Add("Custom");
        }

        if (hasChildren)
        {
            badges.Add("Nested");
        }

        if (hasRestrictions)
        {
            badges.Add("Restricted");
        }

        return badges;
    }

    private static IReadOnlyCollection<SchemaViewerEntryDisplayModel> MergeEntries(
        IReadOnlyCollection<SchemaViewerEntryDisplayModel> baseEntries,
        IReadOnlyCollection<SchemaViewerEntryDisplayModel> overrideEntries)
    {
        if (baseEntries.Count == 0)
        {
            return overrideEntries;
        }

        if (overrideEntries.Count == 0)
        {
            return baseEntries;
        }

        Dictionary<string, SchemaViewerEntryDisplayModel> entries = baseEntries.ToDictionary(entry => entry.Key, StringComparer.Ordinal);
        foreach (SchemaViewerEntryDisplayModel entry in overrideEntries)
        {
            entries[entry.Key] = entry;
        }

        return entries.Values.ToArray();
    }

    private static IReadOnlyCollection<SchemaViewerEntryDisplayModel> ReadRestrictions(JsonElement element)
    {
        List<SchemaViewerEntryDisplayModel> restrictions = [];
        foreach (KeyValuePair<string, string> pair in RestrictionLabels)
        {
            if (!element.TryGetProperty(pair.Key, out JsonElement value))
            {
                continue;
            }

            restrictions.Add(new SchemaViewerEntryDisplayModel
            {
                Key = pair.Key,
                Label = pair.Value,
                Value = ReadValue(value)
            });
        }

        return restrictions;
    }

    private static IReadOnlyCollection<SchemaViewerEntryDisplayModel> ReadMetadata(JsonElement element)
    {
        if (!TryGetObject(element, "metadata", out JsonElement metadata))
        {
            return [];
        }

        List<SchemaViewerEntryDisplayModel> entries = [];
        foreach (JsonProperty property in metadata.EnumerateObject())
        {
            entries.Add(new SchemaViewerEntryDisplayModel
            {
                Key = property.Name,
                Label = property.Name,
                Value = ReadMetadataValue(property.Value)
            });
        }

        return entries;
    }

    private static string ReadMetadataValue(JsonElement element)
    {
        if (element.ValueKind == JsonValueKind.Object &&
            element.TryGetProperty("value", out JsonElement value))
        {
            return ReadValue(value);
        }

        return ReadValue(element);
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
            return "object";
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

    private static string ReadValue(JsonElement element)
    {
        return element.ValueKind == JsonValueKind.String
            ? element.GetString() ?? string.Empty
            : element.GetRawText();
    }
}
