namespace ButterMorph.SchemaDesign;

using System.Globalization;
using System.Text;
using System.Text.Json;
using ButterMorph.Abstractions;

/// <summary>
/// Builds JSON Schema for custom schema type versions.
/// </summary>
public sealed class SchemaTypeSchemaBuilder : ISchemaTypeSchemaBuilder
{
    // JSON text used for map-shaped schema type values.
    private const string MapType = "obj" + "ect";

    /// <summary>
    /// Builds the schema type result.
    /// </summary>
    /// <param name="input">The design input.</param>
    /// <param name="catalog">The available schema types.</param>
    /// <returns>The design result.</returns>
    public SchemaTypeDesignResult Build(SchemaTypeDesignInput input, IReadOnlyCollection<SchemaTypeCatalogItem> catalog)
    {
        input = NormalizeInput(input);
        if (catalog == null)
        {
            catalog = [];
        }

        List<DiagnosticEntry> diagnostics = Validate(input);

        if (diagnostics.Count > 0)
        {
            return new SchemaTypeDesignResult
            {
                Succeeded = false,
                Diagnostics = diagnostics,
                Key = input.Key,
                Name = input.Name,
                Description = input.Description,
                VersionNumber = input.VersionNumber,
                BaseType = input.BaseType,
                Comment = input.Comment
            };
        }

        string jsonSchema = CreateSchema(input, catalog);

        return new SchemaTypeDesignResult
        {
            Succeeded = true,
            Diagnostics = [],
            Key = input.Key.Trim(),
            Name = input.Name.Trim(),
            Description = input.Description.Trim(),
            VersionNumber = input.VersionNumber.Trim(),
            BaseType = input.BaseType.Trim(),
            JsonSchema = jsonSchema,
            Comment = input.Comment
        };
    }

    // Normalizes model-bound string values because browser posts may omit optional fields.
    private static SchemaTypeDesignInput NormalizeInput(SchemaTypeDesignInput input)
    {
        if (input == null)
        {
            input = new SchemaTypeDesignInput();
        }

        return new SchemaTypeDesignInput
        {
            Name = SafeString(input.Name),
            Key = SafeString(input.Key),
            Description = SafeString(input.Description),
            VersionNumber = SafeString(input.VersionNumber),
            BaseType = SafeString(input.BaseType),
            MinLength = SafeString(input.MinLength),
            MaxLength = SafeString(input.MaxLength),
            Pattern = SafeString(input.Pattern),
            Minimum = SafeString(input.Minimum),
            Maximum = SafeString(input.Maximum),
            Precision = SafeString(input.Precision),
            Scale = SafeString(input.Scale),
            MinItems = SafeString(input.MinItems),
            MaxItems = SafeString(input.MaxItems),
            AllowedValuesJson = SafeString(input.AllowedValuesJson),
            ArrayItemType = SafeString(input.ArrayItemType),
            ArrayItemTypeVersionId = SafeString(input.ArrayItemTypeVersionId),
            PayloadSchemaJson = SafeString(input.PayloadSchemaJson),
            Comment = SafeString(input.Comment)
        };
    }

    // Converts model-bound null strings to empty values.
    private static string SafeString(string value)
    {
        if (value == null)
        {
            return string.Empty;
        }

        return value;
    }

    // Validates required schema type fields.
    private static List<DiagnosticEntry> Validate(SchemaTypeDesignInput input)
    {
        List<DiagnosticEntry> diagnostics = [];

        if (string.IsNullOrWhiteSpace(input.Key))
        {
            diagnostics.Add(CreateDiagnostic("BMSD100", "Type key is required.", "Key"));
        }

        if (string.IsNullOrWhiteSpace(input.Name))
        {
            diagnostics.Add(CreateDiagnostic("BMSD101", "Type name is required.", "Name"));
        }

        if (string.IsNullOrWhiteSpace(input.VersionNumber))
        {
            diagnostics.Add(CreateDiagnostic("BMSD102", "Version number is required.", "VersionNumber"));
        }

        if (string.IsNullOrWhiteSpace(input.BaseType))
        {
            diagnostics.Add(CreateDiagnostic("BMSD103", "Base type is required.", "BaseType"));
        }

        if (string.Equals(input.BaseType, MapType, StringComparison.OrdinalIgnoreCase) && !string.IsNullOrWhiteSpace(input.PayloadSchemaJson))
        {
            TryParseJson(input.PayloadSchemaJson, diagnostics, "PayloadSchemaJson");
        }

        if (!string.IsNullOrWhiteSpace(input.AllowedValuesJson))
        {
            TryParseJson(input.AllowedValuesJson, diagnostics, "AllowedValuesJson");
        }

        return diagnostics;
    }

    // Creates a diagnostic entry.
    private static DiagnosticEntry CreateDiagnostic(string code, string message, string path)
    {
        return new DiagnosticEntry
        {
            Code = code,
            Message = message,
            Severity = "Error",
            Path = path
        };
    }

    // Attempts to parse JSON and adds diagnostics when invalid.
    private static void TryParseJson(string json, List<DiagnosticEntry> diagnostics, string path)
    {
        try
        {
            using JsonDocument document = JsonDocument.Parse(json);
        }
        catch (JsonException exception)
        {
            diagnostics.Add(CreateDiagnostic("BMSD104", exception.Message, path));
        }
    }

    // Creates compact JSON Schema for the schema type.
    private static string CreateSchema(SchemaTypeDesignInput input, IReadOnlyCollection<SchemaTypeCatalogItem> catalog)
    {
        if (string.Equals(input.BaseType, MapType, StringComparison.OrdinalIgnoreCase) && !string.IsNullOrWhiteSpace(input.PayloadSchemaJson))
        {
            return AddSchemaIdentity(input.PayloadSchemaJson, input.Key, input.Name, input.Description);
        }

        using MemoryStream stream = new();
        using Utf8JsonWriter writer = new(stream, new JsonWriterOptions { Indented = false });
        writer.WriteStartObject();
        writer.WriteString("key", input.Key.Trim());
        writer.WriteString("name", input.Name.Trim());
        writer.WriteString("type", NormalizeBaseType(input.BaseType));
        WriteDescription(writer, input.Description);
        WriteConstraints(writer, input);
        WriteAllowedValues(writer, input.AllowedValuesJson);
        WriteArrayItems(writer, input, catalog);
        writer.WriteEndObject();
        writer.Flush();

        return Encoding.UTF8.GetString(stream.ToArray());
    }

    // Adds canonical ButterMorph schema identity to a JSON Schema root.
    private static string AddSchemaIdentity(string jsonSchema, string key, string name, string description)
    {
        using JsonDocument document = JsonDocument.Parse(jsonSchema);
        using MemoryStream stream = new();
        using Utf8JsonWriter writer = new(stream, new JsonWriterOptions { Indented = false });

        writer.WriteStartObject();
        writer.WriteString("key", key.Trim());
        writer.WriteString("name", name.Trim());
        WriteDescription(writer, description);

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

    // Normalizes the selected type.
    private static string NormalizeBaseType(string baseType)
    {
        if (string.Equals(baseType, MapType, StringComparison.OrdinalIgnoreCase))
        {
            return MapType;
        }

        return baseType.Trim();
    }

    // Writes optional description.
    private static void WriteDescription(Utf8JsonWriter writer, string description)
    {
        if (!string.IsNullOrWhiteSpace(description))
        {
            writer.WriteString("description", description.Trim());
        }
    }

    // Writes base-type constraints.
    private static void WriteConstraints(Utf8JsonWriter writer, SchemaTypeDesignInput input)
    {
        WriteInteger(writer, "minLength", input.MinLength);
        WriteInteger(writer, "maxLength", input.MaxLength);
        WriteString(writer, "pattern", input.Pattern);
        WriteDecimal(writer, "minimum", input.Minimum);
        WriteDecimal(writer, "maximum", input.Maximum);
        WriteInteger(writer, "precision", input.Precision);
        WriteInteger(writer, "scale", input.Scale);
        WriteInteger(writer, "minItems", input.MinItems);
        WriteInteger(writer, "maxItems", input.MaxItems);
    }

    // Writes array item schema.
    private static void WriteArrayItems(Utf8JsonWriter writer, SchemaTypeDesignInput input, IReadOnlyCollection<SchemaTypeCatalogItem> catalog)
    {
        if (!string.Equals(input.BaseType, "array", StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        writer.WritePropertyName("items");
        writer.WriteStartObject();

        SchemaTypeCatalogItem catalogItem = FindCatalogItem(input.ArrayItemTypeVersionId, catalog);
        if (!string.IsNullOrWhiteSpace(input.ArrayItemTypeVersionId) && !string.IsNullOrWhiteSpace(catalogItem.TypeVersionId))
        {
            writer.WriteString("$ref", "#/$defs/" + catalogItem.TypeVersionId);
            writer.WriteString("type", catalogItem.BaseType);
            writer.WriteString("typeId", catalogItem.TypeId);
            writer.WriteString("typeVersionId", catalogItem.TypeVersionId);
        }
        else
        {
            writer.WriteString("type", ResolveArrayItemType(input.ArrayItemType));
        }

        writer.WriteEndObject();

        if (!string.IsNullOrWhiteSpace(catalogItem.TypeVersionId) && !string.IsNullOrWhiteSpace(catalogItem.JsonSchema))
        {
            writer.WritePropertyName("$defs");
            writer.WriteStartObject();
            writer.WritePropertyName(catalogItem.TypeVersionId);
            using JsonDocument document = JsonDocument.Parse(catalogItem.JsonSchema);
            document.RootElement.WriteTo(writer);
            writer.WriteEndObject();
        }
    }

    // Finds a catalog item by version identifier.
    private static SchemaTypeCatalogItem FindCatalogItem(string typeVersionId, IReadOnlyCollection<SchemaTypeCatalogItem> catalog)
    {
        foreach (SchemaTypeCatalogItem item in catalog)
        {
            if (string.Equals(item.TypeVersionId, typeVersionId, StringComparison.OrdinalIgnoreCase))
            {
                return item;
            }
        }

        return new SchemaTypeCatalogItem();
    }

    // Resolves the array item base type.
    private static string ResolveArrayItemType(string arrayItemType)
    {
        if (string.IsNullOrWhiteSpace(arrayItemType))
        {
            return "string";
        }

        return NormalizeBaseType(arrayItemType);
    }

    // Writes optional string keyword.
    private static void WriteString(Utf8JsonWriter writer, string key, string value)
    {
        if (!string.IsNullOrWhiteSpace(value))
        {
            writer.WriteString(key, value.Trim());
        }
    }

    // Writes optional integer keyword.
    private static void WriteInteger(Utf8JsonWriter writer, string key, string value)
    {
        if (int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out int parsed))
        {
            writer.WriteNumber(key, parsed);
        }
    }

    // Writes optional decimal keyword.
    private static void WriteDecimal(Utf8JsonWriter writer, string key, string value)
    {
        if (decimal.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out decimal parsed))
        {
            writer.WriteNumber(key, parsed);
        }
    }

    // Writes enum values from JSON array.
    private static void WriteAllowedValues(Utf8JsonWriter writer, string allowedValuesJson)
    {
        if (string.IsNullOrWhiteSpace(allowedValuesJson))
        {
            return;
        }

        using JsonDocument document = JsonDocument.Parse(allowedValuesJson);
        if (document.RootElement.ValueKind != JsonValueKind.Array || document.RootElement.GetArrayLength() == 0)
        {
            return;
        }

        writer.WritePropertyName("enum");
        document.RootElement.WriteTo(writer);
    }
}

