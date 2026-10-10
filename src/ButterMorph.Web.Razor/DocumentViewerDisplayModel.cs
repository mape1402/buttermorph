namespace ButterMorph.Web.Razor;

using System.Globalization;
using System.Text;
using ButterMorph.Abstractions;

internal sealed class DocumentViewerDisplayModel
{
    public string Key { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string Version { get; set; } = string.Empty;

    public string DocumentType { get; set; } = string.Empty;

    public DocumentViewerNodeDisplayModel Root { get; set; } = new();

    public IReadOnlyCollection<DocumentViewerNodeDisplayModel> Items { get; set; } = [];
}

internal sealed class DocumentViewerNodeDisplayModel
{
    public string Id { get; set; } = string.Empty;

    public string Kind { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;

    public string Subtitle { get; set; } = string.Empty;

    public string Path { get; set; } = string.Empty;

    public string DataType { get; set; } = string.Empty;

    public string Expression { get; set; } = string.Empty;

    public string Badge { get; set; } = string.Empty;

    public IReadOnlyCollection<DocumentViewerEntryDisplayModel> Details { get; set; } = [];

    public IReadOnlyCollection<DocumentViewerNodeDisplayModel> Children { get; set; } = [];
}

internal sealed class DocumentViewerEntryDisplayModel
{
    public string Label { get; set; } = string.Empty;

    public string Value { get; set; } = string.Empty;
}

internal static class DocumentViewerDisplayBuilder
{
    internal static DocumentViewerDisplayModel BuildMapping(ButterMorphMappingViewerLoadResult result)
    {
        ITransformationDocument document = result.Document;
        IReadOnlyDictionary<string, string> metadata = document?.Metadata ?? new Dictionary<string, string>();
        string key = ResolveValue(result.Key, ReadMetadata(metadata, "key"), "mapping");
        string name = ResolveValue(result.Name, ReadMetadata(metadata, "name"), key, "Mapping");
        string description = ResolveValue(result.Description, ReadMetadata(metadata, "description"));
        string version = ResolveValue(result.Version, ReadMetadata(metadata, "version"));
        IReadOnlyCollection<ITransformationMapping> mappings = document?.Mappings ?? [];
        Dictionary<string, ITransformationMapping> mappingByPath = mappings
            .Where(mapping => !string.IsNullOrWhiteSpace(mapping.TargetPath))
            .GroupBy(mapping => mapping.TargetPath, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(group => group.Key, group => group.First(), StringComparer.OrdinalIgnoreCase);

        int nextId = 0;
        List<DocumentViewerNodeDisplayModel> items = [];
        IReadOnlyCollection<DocumentViewerNodeDisplayModel> children = result.TargetSchema?.Root?.Children?.Count > 0
            ? ReadMappingSchemaNodes(result.TargetSchema.Root.Children, string.Empty, mappingByPath, items, ref nextId)
            : ReadStandaloneMappings(mappings, items, ref nextId);

        HashSet<string> schemaPaths = items.Select(item => item.Path).Where(path => !string.IsNullOrWhiteSpace(path)).ToHashSet(StringComparer.OrdinalIgnoreCase);
        List<DocumentViewerNodeDisplayModel> allChildren = children.ToList();
        foreach (ITransformationMapping mapping in mappings.Where(mapping => !schemaPaths.Contains(mapping.TargetPath)))
        {
            DocumentViewerNodeDisplayModel extra = CreateMappingNode(mapping.TargetPath, mapping.TargetPath, "mapping", "mapping", mapping, ref nextId);
            allChildren.Add(extra);
            items.Add(extra);
        }

        return new DocumentViewerDisplayModel
        {
            Key = key,
            Name = name,
            Description = description,
            Version = version,
            DocumentType = "mapping",
            Root = new DocumentViewerNodeDisplayModel
            {
                Id = "root",
                Kind = "root",
                Title = "Root",
                Subtitle = ResolveValue(result.TargetSchema?.Name, "Target schema"),
                DataType = "object",
                Children = allChildren
            },
            Items = items
        };
    }

    internal static DocumentViewerDisplayModel BuildValidation(ButterMorphValidationViewerLoadResult result)
    {
        IValidationDocument document = result.Document;
        string key = ResolveValue(result.Key, "validation");
        string name = ResolveValue(result.Name, key, "Validation");
        string description = ResolveValue(result.Description);
        string version = ResolveValue(result.Version);
        IReadOnlyCollection<IValidationStatement> statements = document?.Statements ?? [];

        int nextId = 0;
        List<DocumentViewerNodeDisplayModel> items = [];
        IReadOnlyCollection<DocumentViewerNodeDisplayModel> children = ReadValidationStatements(statements, items, ref nextId);

        return new DocumentViewerDisplayModel
        {
            Key = key,
            Name = name,
            Description = description,
            Version = version,
            DocumentType = "validation",
            Root = new DocumentViewerNodeDisplayModel
            {
                Id = "root",
                Kind = "root",
                Title = "Root",
                Subtitle = statements.Count == 1 ? "1 validation rule" : statements.Count.ToString(CultureInfo.InvariantCulture) + " validation rules",
                DataType = "validation",
                Children = children
            },
            Items = items
        };
    }

    private static IReadOnlyCollection<DocumentViewerNodeDisplayModel> ReadMappingSchemaNodes(
        IReadOnlyCollection<ISchemaNode> schemaNodes,
        string parentPath,
        Dictionary<string, ITransformationMapping> mappingByPath,
        List<DocumentViewerNodeDisplayModel> items,
        ref int nextId)
    {
        List<DocumentViewerNodeDisplayModel> nodes = [];
        foreach (ISchemaNode schemaNode in schemaNodes)
        {
            string path = string.IsNullOrWhiteSpace(parentPath) ? schemaNode.Name : parentPath + "." + schemaNode.Name;
            mappingByPath.TryGetValue(path, out ITransformationMapping mapping);
            DocumentViewerNodeDisplayModel node = CreateMappingNode(
                schemaNode.Name,
                path,
                schemaNode.Kind.ToString().ToLowerInvariant(),
                ResolveSchemaDataType(schemaNode),
                mapping,
                ref nextId);

            node.Children = ReadMappingSchemaNodes(schemaNode.Children, path, mappingByPath, items, ref nextId);
            nodes.Add(node);
            items.Add(node);
        }

        return nodes;
    }

    private static IReadOnlyCollection<DocumentViewerNodeDisplayModel> ReadStandaloneMappings(
        IReadOnlyCollection<ITransformationMapping> mappings,
        List<DocumentViewerNodeDisplayModel> items,
        ref int nextId)
    {
        List<DocumentViewerNodeDisplayModel> nodes = [];
        foreach (ITransformationMapping mapping in mappings)
        {
            DocumentViewerNodeDisplayModel node = CreateMappingNode(mapping.TargetPath, mapping.TargetPath, "mapping", "mapping", mapping, ref nextId);
            nodes.Add(node);
            items.Add(node);
        }

        return nodes;
    }

    private static DocumentViewerNodeDisplayModel CreateMappingNode(
        string title,
        string path,
        string kind,
        string dataType,
        ITransformationMapping mapping,
        ref int nextId)
    {
        string expression = mapping == null ? string.Empty : ExpressionDslFormatter.Write(mapping.SourceExpression);
        string mappingKind = ResolveMappingKind(mapping?.SourceExpression);
        List<DocumentViewerEntryDisplayModel> details =
        [
            Entry("Target path", path),
            Entry("Target type", dataType),
            Entry("Mapping", mappingKind)
        ];

        if (!string.IsNullOrWhiteSpace(expression))
        {
            details.Add(Entry("Expression", expression));
        }

        AddExpressionDetails(details, mapping?.SourceExpression);

        return new DocumentViewerNodeDisplayModel
        {
            Id = "mapping-" + nextId++.ToString(CultureInfo.InvariantCulture),
            Kind = kind,
            Title = title,
            Subtitle = string.IsNullOrWhiteSpace(expression) ? "No mapping configured" : expression,
            Path = path,
            DataType = dataType,
            Expression = expression,
            Badge = mappingKind,
            Details = details
        };
    }

    private static IReadOnlyCollection<DocumentViewerNodeDisplayModel> ReadValidationStatements(
        IReadOnlyCollection<IValidationStatement> statements,
        List<DocumentViewerNodeDisplayModel> items,
        ref int nextId)
    {
        List<DocumentViewerNodeDisplayModel> nodes = [];
        int index = 1;
        foreach (IValidationStatement statement in statements)
        {
            DocumentViewerNodeDisplayModel node = CreateValidationNode(statement, index, items, ref nextId);
            nodes.Add(node);
            items.Add(node);
            index++;
        }

        return nodes;
    }

    private static DocumentViewerNodeDisplayModel CreateValidationNode(
        IValidationStatement statement,
        int index,
        List<DocumentViewerNodeDisplayModel> items,
        ref int nextId)
    {
        if (statement is IValidationForEach forEach)
        {
            string source = ExpressionDslFormatter.Write(forEach.SourceExpression);
            List<DocumentViewerEntryDisplayModel> details =
            [
                Entry("For each", source),
                Entry("Alias", string.IsNullOrWhiteSpace(forEach.ItemAlias) ? "item" : forEach.ItemAlias),
                Entry("Rules", forEach.Statements.Count.ToString(CultureInfo.InvariantCulture))
            ];

            DocumentViewerNodeDisplayModel node = new()
            {
                Id = "validation-" + nextId++.ToString(CultureInfo.InvariantCulture),
                Kind = "foreach",
                Title = "For each " + source,
                Subtitle = "as " + (string.IsNullOrWhiteSpace(forEach.ItemAlias) ? "item" : forEach.ItemAlias),
                DataType = "foreach",
                Expression = source,
                Badge = "For each",
                Details = details
            };
            node.Children = ReadValidationStatements(forEach.Statements, items, ref nextId);
            return node;
        }

        if (statement is IValidationAssertion assertion)
        {
            string expression = ExpressionDslFormatter.Write(assertion.Expression);
            string kind = ResolveValidationKind(assertion.Expression);
            List<DocumentViewerEntryDisplayModel> details =
            [
                Entry("Message", assertion.Message),
                Entry("Expression", expression)
            ];

            if (!string.IsNullOrWhiteSpace(assertion.Path))
            {
                details.Add(Entry("Path", assertion.Path));
            }

            AddExpressionDetails(details, assertion.Expression);
            return new DocumentViewerNodeDisplayModel
            {
                Id = "validation-" + nextId++.ToString(CultureInfo.InvariantCulture),
                Kind = "assertion",
                Title = ResolveValue(assertion.Message, "Rule " + index.ToString(CultureInfo.InvariantCulture)),
                Subtitle = expression,
                Path = assertion.Path,
                DataType = "assertion",
                Expression = expression,
                Badge = kind,
                Details = details
            };
        }

        return new DocumentViewerNodeDisplayModel
        {
            Id = "validation-" + nextId++.ToString(CultureInfo.InvariantCulture),
            Kind = "statement",
            Title = "Rule " + index.ToString(CultureInfo.InvariantCulture),
            Subtitle = statement.GetType().Name,
            Badge = "Rule"
        };
    }

    private static void AddExpressionDetails(List<DocumentViewerEntryDisplayModel> details, ITransformationExpression expression)
    {
        if (expression is IConditionalExpression conditional)
        {
            details.Add(Entry("When", ExpressionDslFormatter.Write(conditional.Condition)));
            details.Add(Entry("Then", ExpressionDslFormatter.Write(conditional.ThenExpression)));
            details.Add(Entry("Otherwise", ExpressionDslFormatter.Write(conditional.ElseExpression)));
            return;
        }

        if (expression is ICollectionProjectionExpression projection)
        {
            details.Add(Entry("For each", ExpressionDslFormatter.Write(projection.SourceExpression)));
            details.Add(Entry("Alias", projection.ItemAlias));
            details.Add(Entry("Map", ExpressionDslFormatter.Write(projection.BodyExpression)));
        }
    }

    private static string ResolveMappingKind(ITransformationExpression expression)
    {
        if (expression == null)
        {
            return "Unmapped";
        }

        if (expression is IConditionalExpression)
        {
            return "Conditional";
        }

        if (expression is ICollectionProjectionExpression)
        {
            return "For each";
        }

        return "Expression";
    }

    private static string ResolveValidationKind(ITransformationExpression expression)
    {
        if (expression is IConditionalExpression)
        {
            return "When";
        }

        if (expression is IFunctionCallExpression function)
        {
            if (string.Equals(function.FunctionKey, "and", StringComparison.OrdinalIgnoreCase))
            {
                return "All";
            }

            if (string.Equals(function.FunctionKey, "or", StringComparison.OrdinalIgnoreCase))
            {
                return "Any";
            }

            if (function.Arguments.FirstOrDefault() is IPathExpression)
            {
                return "Field rule";
            }
        }

        return "Assertion";
    }

    private static string ResolveSchemaDataType(ISchemaNode node)
    {
        if (node.Kind == SchemaNodeKind.Array)
        {
            return "array";
        }

        if (node.Kind == SchemaNodeKind.Object)
        {
            return "object";
        }

        return ResolveValue(node.DataType, "scalar");
    }

    private static DocumentViewerEntryDisplayModel Entry(string label, string value)
    {
        return new DocumentViewerEntryDisplayModel
        {
            Label = label,
            Value = value ?? string.Empty
        };
    }

    private static string ReadMetadata(IReadOnlyDictionary<string, string> metadata, string key)
    {
        return metadata.TryGetValue(key, out string value) ? value : string.Empty;
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
}

internal static class ExpressionDslFormatter
{
    internal static string Write(ITransformationExpression expression)
    {
        if (expression == null)
        {
            return string.Empty;
        }

        if (expression is IPathExpression path)
        {
            return path.Path;
        }

        if (expression is IScalarLiteralExpression scalar)
        {
            return WriteScalar(scalar.Value);
        }

        if (expression is IScalarCollectionLiteralExpression scalarCollection)
        {
            return "scalars(" + string.Join(", ", scalarCollection.Values.Select(WriteScalar)) + ")";
        }

        if (expression is IFunctionCallExpression function)
        {
            return function.FunctionKey + "(" + string.Join(", ", function.Arguments.Select(Write)) + ")";
        }

        if (expression is IConditionalExpression conditional)
        {
            return "when(" + Write(conditional.Condition) + ", " + Write(conditional.ThenExpression) + ", " + Write(conditional.ElseExpression) + ")";
        }

        if (expression is ICollectionProjectionExpression projection)
        {
            return "project " + Write(projection.SourceExpression) + " as " + projection.ItemAlias + " => " + Write(projection.BodyExpression);
        }

        if (expression is IObjectExpression map)
        {
            return "{ " + string.Join(", ", map.Properties.Select(WriteProperty)) + " }";
        }

        if (expression is IArrayExpression ordered)
        {
            return "[" + string.Join(", ", ordered.Items.Select(Write)) + "]";
        }

        return expression.Kind.ToString();
    }

    private static string WriteProperty(IObjectPropertyExpression property)
    {
        return property.Name + ": " + Write(property.Expression);
    }

    private static string WriteScalar(IScalarValue value)
    {
        if (value == null || value.IsNull)
        {
            return "null";
        }

        if (string.Equals(value.DataType, "Boolean", StringComparison.Ordinal))
        {
            return value.RawValue.ToLower(CultureInfo.InvariantCulture);
        }

        if (string.Equals(value.DataType, "Number", StringComparison.Ordinal))
        {
            return value.RawValue;
        }

        return WriteString(value.RawValue);
    }

    private static string WriteString(string value)
    {
        StringBuilder builder = new();
        builder.Append('"');

        foreach (char character in value ?? string.Empty)
        {
            if (character == '"')
            {
                builder.Append("\\\"");
            }
            else if (character == '\\')
            {
                builder.Append("\\\\");
            }
            else if (character == '\n')
            {
                builder.Append("\\n");
            }
            else if (character == '\r')
            {
                builder.Append("\\r");
            }
            else if (character == '\t')
            {
                builder.Append("\\t");
            }
            else
            {
                builder.Append(character);
            }
        }

        builder.Append('"');
        return builder.ToString();
    }
}
