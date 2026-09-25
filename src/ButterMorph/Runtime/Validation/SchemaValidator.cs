namespace ButterMorph.Validation;

using System.Globalization;
using System.Text.Json;
using System.Text.RegularExpressions;
using ButterMorph.Abstractions;
using ButterMorph.Core;
using ButterMorph.Functions;

/// <summary>
/// Validates structure graphs against ButterMorph schemas and schema metadata constraints.
/// </summary>
public sealed class SchemaValidator : ISchemaValidator
{
    /// <summary>
    /// Validates a request against schema rules.
    /// </summary>
    /// <param name="request">The validation request.</param>
    /// <returns>The validation result.</returns>
    public ValidationResult Validate(ValidationRequest request)
    {
        List<DiagnosticEntry> diagnostics = [];

        if (request.Schema == null)
        {
            diagnostics.Add(CreateDiagnostic("BMSV000", "Validation schema is required.", string.Empty));
            return CreateResult(diagnostics);
        }

        IStructureGraph graph = ResolveGraph(request);

        if (graph == null)
        {
            diagnostics.Add(CreateDiagnostic("BMSV000", "Validation payload graph is required.", string.Empty));
            return CreateResult(diagnostics);
        }

        Dictionary<string, ISchemaNode> definitions = CreateDefinitions(request.Schema);
        ValidateNode(request.Schema.Root, graph.Root, "$root", diagnostics, definitions);
        return CreateResult(diagnostics);
    }

    private static void ValidateNode(
        ISchemaNode schemaNode,
        IStructureNode payloadNode,
        string path,
        List<DiagnosticEntry> diagnostics,
        IReadOnlyDictionary<string, ISchemaNode> definitions)
    {
        if (TryResolveReference(schemaNode, definitions, out ISchemaNode resolvedNode))
        {
            ValidateNode(resolvedNode, payloadNode, path, diagnostics, definitions);
            return;
        }

        if (schemaNode.Kind == SchemaNodeKind.Object)
        {
            ValidateObject(schemaNode, payloadNode, path, diagnostics, definitions);
            return;
        }

        if (schemaNode.Kind == SchemaNodeKind.Array)
        {
            ValidateArray(schemaNode, payloadNode, path, diagnostics, definitions);
            return;
        }

        ValidateScalar(schemaNode, payloadNode, path, diagnostics);
    }

    private static void ValidateObject(
        ISchemaNode schemaNode,
        IStructureNode payloadNode,
        string path,
        List<DiagnosticEntry> diagnostics,
        IReadOnlyDictionary<string, ISchemaNode> definitions)
    {
        if (payloadNode.Kind != StructureNodeKind.Object)
        {
            diagnostics.Add(CreateDiagnostic("BMSV002", $"Expected object at '{path}'.", path));
            return;
        }

        foreach (ISchemaNode childSchema in schemaNode.Children)
        {
            string childPath = CreateChildPath(path, childSchema.Name);
            IStructureNode childPayload = FindChild(payloadNode, childSchema.Name);

            if (childPayload == null)
            {
                if (childSchema.IsRequired)
                {
                    diagnostics.Add(CreateDiagnostic("BMSV001", $"Required field '{childPath}' is missing.", childPath));
                }

                continue;
            }

            ValidateNode(childSchema, childPayload, childPath, diagnostics, definitions);
        }
    }

    private static void ValidateArray(
        ISchemaNode schemaNode,
        IStructureNode payloadNode,
        string path,
        List<DiagnosticEntry> diagnostics,
        IReadOnlyDictionary<string, ISchemaNode> definitions)
    {
        if (payloadNode.Kind != StructureNodeKind.Array)
        {
            diagnostics.Add(CreateDiagnostic("BMSV002", $"Expected array at '{path}'.", path));
            return;
        }

        ValidateItemCount(schemaNode, payloadNode, path, diagnostics);
        ISchemaNode itemSchema = ResolveItemSchema(schemaNode);

        if (itemSchema == null)
        {
            return;
        }

        int index = 0;

        foreach (IStructureNode child in payloadNode.Children)
        {
            ValidateNode(itemSchema, child, path + "[" + index.ToString(CultureInfo.InvariantCulture) + "]", diagnostics, definitions);
            index++;
        }
    }

    private static void ValidateScalar(ISchemaNode schemaNode, IStructureNode payloadNode, string path, List<DiagnosticEntry> diagnostics)
    {
        if (payloadNode is not IScalarStructureNode scalarNode)
        {
            diagnostics.Add(CreateDiagnostic("BMSV002", $"Expected scalar at '{path}'.", path));
            return;
        }

        if (scalarNode.Value.IsNull)
        {
            if (schemaNode.IsRequired)
            {
                diagnostics.Add(CreateDiagnostic("BMSV001", $"Required field '{path}' cannot be null.", path));
            }

            return;
        }

        if (!IsScalarTypeCompatible(schemaNode.DataType, scalarNode.Value))
        {
            diagnostics.Add(CreateDiagnostic("BMSV002", $"Value at '{path}' must be '{schemaNode.DataType}'.", path));
            return;
        }

        ValidateStringConstraints(schemaNode, scalarNode.Value, path, diagnostics);
        ValidateNumberConstraints(schemaNode, scalarNode.Value, path, diagnostics);
        ValidateTemporalConstraints(schemaNode, scalarNode.Value, path, diagnostics);
        ValidateEnumConstraint(schemaNode, scalarNode.Value, path, diagnostics);
    }

    private static void ValidateStringConstraints(ISchemaNode schemaNode, IScalarValue value, string path, List<DiagnosticEntry> diagnostics)
    {
        if (!string.Equals(schemaNode.DataType, "string", StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        string text = value.RawValue ?? string.Empty;

        if (TryReadInt(schemaNode, "minLength", out int minimumLength) && text.Length < minimumLength)
        {
            diagnostics.Add(CreateDiagnostic("BMSV003", $"Value at '{path}' must have at least {minimumLength.ToString(CultureInfo.InvariantCulture)} characters.", path));
        }

        if (TryReadInt(schemaNode, "maxLength", out int maximumLength) && text.Length > maximumLength)
        {
            diagnostics.Add(CreateDiagnostic("BMSV004", $"Value at '{path}' must have at most {maximumLength.ToString(CultureInfo.InvariantCulture)} characters.", path));
        }

        if (schemaNode.Metadata.TryGetValue("pattern", out string pattern) && !string.IsNullOrWhiteSpace(pattern) && !Regex.IsMatch(text, pattern))
        {
            diagnostics.Add(CreateDiagnostic("BMSV010", $"Value at '{path}' does not match the configured pattern.", path));
        }
    }

    private static void ValidateNumberConstraints(ISchemaNode schemaNode, IScalarValue value, string path, List<DiagnosticEntry> diagnostics)
    {
        if (!IsNumberSchemaType(schemaNode.DataType))
        {
            return;
        }

        if (!decimal.TryParse(value.RawValue, NumberStyles.Number, CultureInfo.InvariantCulture, out decimal number))
        {
            diagnostics.Add(CreateDiagnostic("BMSV002", $"Value at '{path}' must be numeric.", path));
            return;
        }

        if (TryReadDecimal(schemaNode, "minimum", out decimal minimum) && number < minimum)
        {
            diagnostics.Add(CreateDiagnostic("BMSV005", $"Value at '{path}' must be greater than or equal to {minimum.ToString(CultureInfo.InvariantCulture)}.", path));
        }

        if (TryReadDecimal(schemaNode, "maximum", out decimal maximum) && number > maximum)
        {
            diagnostics.Add(CreateDiagnostic("BMSV006", $"Value at '{path}' must be less than or equal to {maximum.ToString(CultureInfo.InvariantCulture)}.", path));
        }
    }

    private static void ValidateTemporalConstraints(ISchemaNode schemaNode, IScalarValue value, string path, List<DiagnosticEntry> diagnostics)
    {
        string schemaType = NormalizeTemporalSchemaType(schemaNode.DataType);

        if (string.IsNullOrWhiteSpace(schemaType))
        {
            return;
        }

        if (string.Equals(schemaType, "date", StringComparison.Ordinal))
        {
            ValidateDateConstraints(schemaNode, value.RawValue, path, diagnostics);
            return;
        }

        if (string.Equals(schemaType, "datetime", StringComparison.Ordinal))
        {
            ValidateDateTimeConstraints(schemaNode, value.RawValue, path, diagnostics);
            return;
        }

        if (string.Equals(schemaType, "time", StringComparison.Ordinal))
        {
            ValidateTimeConstraints(schemaNode, value.RawValue, path, diagnostics);
            return;
        }

        ValidateTimeSpanConstraints(schemaNode, value.RawValue, path, diagnostics);
    }

    private static void ValidateDateConstraints(ISchemaNode schemaNode, string rawValue, string path, List<DiagnosticEntry> diagnostics)
    {
        DateFunctionTools dates = new();

        if (!dates.TryParseDate(rawValue, out DateOnly value))
        {
            diagnostics.Add(CreateDiagnostic("BMSV002", $"Value at '{path}' must be 'date'.", path));
            return;
        }

        if (TryReadTemporalText(schemaNode, "minimum", "minDate", out string minimumText) &&
            dates.TryParseDate(minimumText, out DateOnly minimum) &&
            value < minimum)
        {
            diagnostics.Add(CreateDiagnostic("BMSV011", $"Value at '{path}' must be on or after {minimum.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)}.", path));
        }

        if (TryReadTemporalText(schemaNode, "maximum", "maxDate", out string maximumText) &&
            dates.TryParseDate(maximumText, out DateOnly maximum) &&
            value > maximum)
        {
            diagnostics.Add(CreateDiagnostic("BMSV012", $"Value at '{path}' must be on or before {maximum.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)}.", path));
        }
    }

    private static void ValidateDateTimeConstraints(ISchemaNode schemaNode, string rawValue, string path, List<DiagnosticEntry> diagnostics)
    {
        DateFunctionTools dates = new();

        if (!dates.TryParseDateTime(rawValue, out DateTimeOffset value))
        {
            diagnostics.Add(CreateDiagnostic("BMSV002", $"Value at '{path}' must be 'datetime'.", path));
            return;
        }

        if (TryReadTemporalText(schemaNode, "minimum", "minDateTime", out string minimumText) &&
            dates.TryParseDateTime(minimumText, out DateTimeOffset minimum) &&
            value < minimum)
        {
            diagnostics.Add(CreateDiagnostic("BMSV011", $"Value at '{path}' must be on or after {minimum.ToString("O", CultureInfo.InvariantCulture)}.", path));
        }

        if (TryReadTemporalText(schemaNode, "maximum", "maxDateTime", out string maximumText) &&
            dates.TryParseDateTime(maximumText, out DateTimeOffset maximum) &&
            value > maximum)
        {
            diagnostics.Add(CreateDiagnostic("BMSV012", $"Value at '{path}' must be on or before {maximum.ToString("O", CultureInfo.InvariantCulture)}.", path));
        }
    }

    private static void ValidateTimeConstraints(ISchemaNode schemaNode, string rawValue, string path, List<DiagnosticEntry> diagnostics)
    {
        DateFunctionTools dates = new();

        if (!dates.TryParseTime(rawValue, out TimeOnly value))
        {
            diagnostics.Add(CreateDiagnostic("BMSV002", $"Value at '{path}' must be 'time'.", path));
            return;
        }

        if (TryReadTemporalText(schemaNode, "minimum", "minTime", out string minimumText) &&
            dates.TryParseTime(minimumText, out TimeOnly minimum) &&
            value < minimum)
        {
            diagnostics.Add(CreateDiagnostic("BMSV011", $"Value at '{path}' must be at or after {minimum.ToString("HH:mm:ss", CultureInfo.InvariantCulture)}.", path));
        }

        if (TryReadTemporalText(schemaNode, "maximum", "maxTime", out string maximumText) &&
            dates.TryParseTime(maximumText, out TimeOnly maximum) &&
            value > maximum)
        {
            diagnostics.Add(CreateDiagnostic("BMSV012", $"Value at '{path}' must be at or before {maximum.ToString("HH:mm:ss", CultureInfo.InvariantCulture)}.", path));
        }
    }

    private static void ValidateTimeSpanConstraints(ISchemaNode schemaNode, string rawValue, string path, List<DiagnosticEntry> diagnostics)
    {
        DateFunctionTools dates = new();

        if (!dates.TryParseTimeSpan(rawValue, out TimeSpan value))
        {
            diagnostics.Add(CreateDiagnostic("BMSV002", $"Value at '{path}' must be 'timespan'.", path));
            return;
        }

        if (TryReadTemporalText(schemaNode, "minimum", "minDuration", out string minimumText) &&
            dates.TryParseTimeSpan(minimumText, out TimeSpan minimum) &&
            value < minimum)
        {
            diagnostics.Add(CreateDiagnostic("BMSV011", $"Value at '{path}' must be greater than or equal to {minimum.ToString("c", CultureInfo.InvariantCulture)}.", path));
        }

        if (TryReadTemporalText(schemaNode, "maximum", "maxDuration", out string maximumText) &&
            dates.TryParseTimeSpan(maximumText, out TimeSpan maximum) &&
            value > maximum)
        {
            diagnostics.Add(CreateDiagnostic("BMSV012", $"Value at '{path}' must be less than or equal to {maximum.ToString("c", CultureInfo.InvariantCulture)}.", path));
        }
    }

    private static void ValidateItemCount(ISchemaNode schemaNode, IStructureNode payloadNode, string path, List<DiagnosticEntry> diagnostics)
    {
        if (TryReadInt(schemaNode, "minItems", out int minimumItems) && payloadNode.Children.Count < minimumItems)
        {
            diagnostics.Add(CreateDiagnostic("BMSV007", $"Array at '{path}' must contain at least {minimumItems.ToString(CultureInfo.InvariantCulture)} items.", path));
        }

        if (TryReadInt(schemaNode, "maxItems", out int maximumItems) && payloadNode.Children.Count > maximumItems)
        {
            diagnostics.Add(CreateDiagnostic("BMSV008", $"Array at '{path}' must contain at most {maximumItems.ToString(CultureInfo.InvariantCulture)} items.", path));
        }
    }

    private static void ValidateEnumConstraint(ISchemaNode schemaNode, IScalarValue value, string path, List<DiagnosticEntry> diagnostics)
    {
        if (!schemaNode.Metadata.TryGetValue("enum", out string enumText) || string.IsNullOrWhiteSpace(enumText))
        {
            return;
        }

        try
        {
            using JsonDocument document = JsonDocument.Parse(enumText);

            if (document.RootElement.ValueKind != JsonValueKind.Array)
            {
                return;
            }

            foreach (JsonElement item in document.RootElement.EnumerateArray())
            {
                if (ScalarMatchesEnumItem(value, item))
                {
                    return;
                }
            }

            diagnostics.Add(CreateDiagnostic("BMSV009", $"Value at '{path}' is not in the configured enum.", path));
        }
        catch (JsonException)
        {
            diagnostics.Add(CreateDiagnostic("BMSV009", $"Enum constraint at '{path}' is not valid JSON.", path));
        }
    }

    private static bool ScalarMatchesEnumItem(IScalarValue value, JsonElement item)
    {
        if (item.ValueKind == JsonValueKind.String)
        {
            return string.Equals(value.RawValue, item.GetString(), StringComparison.Ordinal);
        }

        if (item.ValueKind == JsonValueKind.Number)
        {
            return decimal.TryParse(value.RawValue, NumberStyles.Number, CultureInfo.InvariantCulture, out decimal scalarNumber) &&
                decimal.TryParse(item.GetRawText(), NumberStyles.Number, CultureInfo.InvariantCulture, out decimal enumNumber) &&
                scalarNumber == enumNumber;
        }

        if (item.ValueKind == JsonValueKind.True || item.ValueKind == JsonValueKind.False)
        {
            return string.Equals(value.RawValue, item.GetBoolean().ToString(CultureInfo.InvariantCulture), StringComparison.OrdinalIgnoreCase);
        }

        if (item.ValueKind == JsonValueKind.Null)
        {
            return value.IsNull;
        }

        return string.Equals(value.RawValue, item.GetRawText(), StringComparison.Ordinal);
    }

    private static Dictionary<string, ISchemaNode> CreateDefinitions(IStructureSchema schema)
    {
        Dictionary<string, ISchemaNode> definitions = new(StringComparer.Ordinal);

        if (!schema.Metadata.TryGetValue("json:$defs", out string definitionsJson) || string.IsNullOrWhiteSpace(definitionsJson))
        {
            return definitions;
        }

        try
        {
            using JsonDocument document = JsonDocument.Parse(definitionsJson);

            if (document.RootElement.ValueKind != JsonValueKind.Object)
            {
                return definitions;
            }

            foreach (JsonProperty definition in document.RootElement.EnumerateObject())
            {
                definitions[definition.Name] = ImportDefinitionNode(definition.Name, definition.Value);
            }
        }
        catch (JsonException)
        {
            return definitions;
        }

        return definitions;
    }

    private static ISchemaNode ImportDefinitionNode(string name, JsonElement element)
    {
        string dataType = ReadDefinitionType(element);
        Dictionary<string, string> metadata = ReadDefinitionMetadata(element);

        if (string.Equals(dataType, "object", StringComparison.OrdinalIgnoreCase))
        {
            List<ISchemaNode> children = [];

            if (element.TryGetProperty("properties", out JsonElement properties) && properties.ValueKind == JsonValueKind.Object)
            {
                foreach (JsonProperty property in properties.EnumerateObject())
                {
                    children.Add(ImportDefinitionNode(property.Name, property.Value));
                }
            }

            return new SchemaNode
            {
                Name = name,
                Kind = SchemaNodeKind.Object,
                DataType = dataType,
                Children = children,
                Metadata = metadata
            };
        }

        if (string.Equals(dataType, "array", StringComparison.OrdinalIgnoreCase))
        {
            List<ISchemaNode> children = [];

            if (element.TryGetProperty("items", out JsonElement items))
            {
                children.Add(ImportDefinitionNode("$item", items));
            }

            return new SchemaNode
            {
                Name = name,
                Kind = SchemaNodeKind.Array,
                DataType = dataType,
                Children = children,
                Metadata = metadata
            };
        }

        return new SchemaNode
        {
            Name = name,
            Kind = SchemaNodeKind.Scalar,
            DataType = dataType,
            Children = [],
            Metadata = metadata
        };
    }

    private static bool TryResolveReference(ISchemaNode schemaNode, IReadOnlyDictionary<string, ISchemaNode> definitions, out ISchemaNode resolvedNode)
    {
        resolvedNode = null;

        if (!schemaNode.Metadata.TryGetValue("$ref", out string reference) || string.IsNullOrWhiteSpace(reference))
        {
            return false;
        }

        string definitionKey = ReadDefinitionKey(reference);

        if (!definitions.TryGetValue(definitionKey, out ISchemaNode definitionNode))
        {
            return false;
        }

        resolvedNode = new SchemaNode
        {
            Name = schemaNode.Name,
            Kind = definitionNode.Kind,
            DataType = definitionNode.DataType,
            IsRequired = schemaNode.IsRequired,
            Children = definitionNode.Children,
            Metadata = MergeReferenceMetadata(definitionNode.Metadata, schemaNode.Metadata)
        };
        return true;
    }

    private static Dictionary<string, string> MergeReferenceMetadata(
        IReadOnlyDictionary<string, string> definitionMetadata,
        IReadOnlyDictionary<string, string> nodeMetadata)
    {
        Dictionary<string, string> metadata = new(definitionMetadata, StringComparer.Ordinal);

        foreach (KeyValuePair<string, string> item in nodeMetadata)
        {
            if (string.Equals(item.Key, "$ref", StringComparison.Ordinal))
            {
                continue;
            }

            metadata[item.Key] = item.Value;
        }

        return metadata;
    }

    private static string ReadDefinitionKey(string reference)
    {
        const string prefix = "#/$defs/";

        if (reference.StartsWith(prefix, StringComparison.Ordinal))
        {
            return reference[prefix.Length..];
        }

        int separator = reference.LastIndexOf("/", StringComparison.Ordinal);

        if (separator >= 0 && separator + 1 < reference.Length)
        {
            return reference[(separator + 1)..];
        }

        return reference;
    }

    private static string ReadDefinitionType(JsonElement element)
    {
        if (element.ValueKind == JsonValueKind.Object &&
            element.TryGetProperty("type", out JsonElement typeElement) &&
            typeElement.ValueKind == JsonValueKind.String)
        {
            string value = typeElement.GetString();

            if (!string.IsNullOrWhiteSpace(value))
            {
                return value;
            }
        }

        return "string";
    }

    private static Dictionary<string, string> ReadDefinitionMetadata(JsonElement element)
    {
        Dictionary<string, string> metadata = new(StringComparer.Ordinal);

        if (element.ValueKind != JsonValueKind.Object)
        {
            return metadata;
        }

        foreach (JsonProperty property in element.EnumerateObject())
        {
            if (IsDefinitionStructuralKeyword(property.Name))
            {
                continue;
            }

            metadata[property.Name] = ReadDefinitionMetadataValue(property.Value);
        }

        return metadata;
    }

    private static bool IsDefinitionStructuralKeyword(string key)
    {
        return string.Equals(key, "type", StringComparison.Ordinal) ||
            string.Equals(key, "properties", StringComparison.Ordinal) ||
            string.Equals(key, "items", StringComparison.Ordinal);
    }

    private static string ReadDefinitionMetadataValue(JsonElement element)
    {
        if (element.ValueKind == JsonValueKind.String)
        {
            return element.GetString();
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

    private static bool IsScalarTypeCompatible(string schemaType, IScalarValue value)
    {
        if (string.IsNullOrWhiteSpace(schemaType))
        {
            return true;
        }

        if (string.Equals(schemaType, "string", StringComparison.OrdinalIgnoreCase))
        {
            return string.Equals(value.DataType, "String", StringComparison.OrdinalIgnoreCase);
        }

        if (string.Equals(schemaType, "boolean", StringComparison.OrdinalIgnoreCase))
        {
            return string.Equals(value.DataType, "Boolean", StringComparison.OrdinalIgnoreCase);
        }

        if (string.Equals(schemaType, "number", StringComparison.OrdinalIgnoreCase))
        {
            return string.Equals(value.DataType, "Number", StringComparison.OrdinalIgnoreCase);
        }

        if (string.Equals(schemaType, "integer", StringComparison.OrdinalIgnoreCase))
        {
            return string.Equals(value.DataType, "Number", StringComparison.OrdinalIgnoreCase) &&
                decimal.TryParse(value.RawValue, NumberStyles.Number, CultureInfo.InvariantCulture, out decimal number) &&
                decimal.Truncate(number) == number;
        }

        string temporalSchemaType = NormalizeTemporalSchemaType(schemaType);

        if (!string.IsNullOrWhiteSpace(temporalSchemaType))
        {
            return IsTemporalScalarCompatible(temporalSchemaType, value);
        }

        return true;
    }

    private static bool IsTemporalScalarCompatible(string schemaType, IScalarValue value)
    {
        if (string.Equals(value.DataType, schemaType, StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        if (!string.Equals(value.DataType, "String", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        DateFunctionTools dates = new();

        return schemaType switch
        {
            "date" => dates.TryParseDate(value.RawValue, out _),
            "datetime" => dates.TryParseDateTime(value.RawValue, out _),
            "time" => dates.TryParseTime(value.RawValue, out _),
            "timespan" => dates.TryParseTimeSpan(value.RawValue, out _),
            _ => true
        };
    }

    private static string NormalizeTemporalSchemaType(string schemaType)
    {
        if (string.Equals(schemaType, "date", StringComparison.OrdinalIgnoreCase))
        {
            return "date";
        }

        if (string.Equals(schemaType, "datetime", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(schemaType, "dateTime", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(schemaType, "date-time", StringComparison.OrdinalIgnoreCase))
        {
            return "datetime";
        }

        if (string.Equals(schemaType, "time", StringComparison.OrdinalIgnoreCase))
        {
            return "time";
        }

        if (string.Equals(schemaType, "timespan", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(schemaType, "timeSpan", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(schemaType, "duration", StringComparison.OrdinalIgnoreCase))
        {
            return "timespan";
        }

        return string.Empty;
    }

    private static bool IsNumberSchemaType(string schemaType)
    {
        return string.Equals(schemaType, "number", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(schemaType, "integer", StringComparison.OrdinalIgnoreCase);
    }

    private static IStructureGraph ResolveGraph(ValidationRequest request)
    {
        if (request.SourceGraph != null)
        {
            return request.SourceGraph;
        }

        string alias = string.IsNullOrWhiteSpace(request.PayloadAlias) ? "source" : request.PayloadAlias;

        if (request.Sources.TryGetValue(alias, out IStructureGraph graph))
        {
            return graph;
        }

        return null;
    }

    private static IStructureNode FindChild(IStructureNode node, string childName)
    {
        foreach (IStructureNode child in node.Children)
        {
            if (string.Equals(child.Name, childName, StringComparison.Ordinal))
            {
                return child;
            }
        }

        return null;
    }

    private static ISchemaNode ResolveItemSchema(ISchemaNode schemaNode)
    {
        foreach (ISchemaNode child in schemaNode.Children)
        {
            if (string.Equals(child.Name, "$item", StringComparison.Ordinal))
            {
                return child;
            }
        }

        foreach (ISchemaNode child in schemaNode.Children)
        {
            return child;
        }

        return null;
    }

    private static bool TryReadInt(ISchemaNode node, string key, out int value)
    {
        value = 0;

        return node.Metadata.TryGetValue(key, out string text) &&
            int.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out value);
    }

    private static bool TryReadDecimal(ISchemaNode node, string key, out decimal value)
    {
        value = 0;

        return node.Metadata.TryGetValue(key, out string text) &&
            decimal.TryParse(text, NumberStyles.Number, CultureInfo.InvariantCulture, out value);
    }

    private static bool TryReadTemporalText(ISchemaNode node, string primaryKey, string aliasKey, out string value)
    {
        if (node.Metadata.TryGetValue(primaryKey, out value) && !string.IsNullOrWhiteSpace(value))
        {
            return true;
        }

        if (node.Metadata.TryGetValue(aliasKey, out value) && !string.IsNullOrWhiteSpace(value))
        {
            return true;
        }

        value = string.Empty;
        return false;
    }

    private static string CreateChildPath(string path, string childName)
    {
        if (string.Equals(path, "$root", StringComparison.Ordinal))
        {
            return childName;
        }

        return path + "." + childName;
    }

    private static ValidationResult CreateResult(IReadOnlyCollection<DiagnosticEntry> diagnostics)
    {
        return new ValidationResult
        {
            IsValid = diagnostics.Count == 0,
            Diagnostics = diagnostics
        };
    }

    private static DiagnosticEntry CreateDiagnostic(string code, string message, string path)
    {
        return new DiagnosticEntry
        {
            Code = code,
            Message = message,
            Path = path,
            Severity = "Error"
        };
    }
}
