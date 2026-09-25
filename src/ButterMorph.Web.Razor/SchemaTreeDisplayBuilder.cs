namespace ButterMorph.Web.Razor;

using ButterMorph.Abstractions;
using ButterMorph.Design;

// Builds nested schema display trees for the reusable designer.
internal static class SchemaTreeDisplayBuilder
{
    internal static SchemaTreeDisplayNode BuildSource(string sourceKey, ISchemaTreeNode root)
    {
        return BuildNode(root, sourceKey, true, new Dictionary<string, string>());
    }

    internal static SchemaTreeDisplayNode BuildTarget(
        ISchemaTreeNode root,
        IReadOnlyDictionary<string, ConditionalMappingDisplayModel> mappings,
        IReadOnlyDictionary<string, IReadOnlyCollection<string>> diagnostics,
        IReadOnlyDictionary<string, ArrayProjectionDisplayModel> projections)
    {
        return BuildNode(root, string.Empty, false, mappings, diagnostics, projections, new ArrayProjectionDisplayModel());
    }

    private static SchemaTreeDisplayNode BuildNode(
        ISchemaTreeNode node,
        string sourceKey,
        bool isSource,
        IReadOnlyDictionary<string, string> expressions)
    {
        Dictionary<string, ConditionalMappingDisplayModel> mappings = new(StringComparer.Ordinal);

        foreach (KeyValuePair<string, string> expression in expressions)
        {
            mappings[expression.Key] = new ConditionalMappingDisplayModel
            {
                Expression = expression.Value
            };
        }

        return BuildNode(node, sourceKey, isSource, mappings, new Dictionary<string, IReadOnlyCollection<string>>(), new Dictionary<string, ArrayProjectionDisplayModel>(), new ArrayProjectionDisplayModel());
    }

    private static SchemaTreeDisplayNode BuildNode(
        ISchemaTreeNode node,
        string sourceKey,
        bool isSource,
        IReadOnlyDictionary<string, ConditionalMappingDisplayModel> mappings,
        IReadOnlyDictionary<string, IReadOnlyCollection<string>> diagnostics,
        IReadOnlyDictionary<string, ArrayProjectionDisplayModel> projections,
        ArrayProjectionDisplayModel projectionContext)
    {
        string path = node.Path;

        if (isSource)
        {
            path = CreateSourcePath(sourceKey, node.Path);
        }

        ArrayProjectionDisplayModel activeProjection = projectionContext;
        bool isArrayProjection = !isSource && node.Kind == SchemaNodeKind.Array;

        if (isArrayProjection && !projections.TryGetValue(path, out activeProjection))
        {
            activeProjection = new ArrayProjectionDisplayModel
            {
                TargetPath = path,
                Alias = "item"
            };
        }

        List<SchemaTreeDisplayNode> children = [];

        foreach (ISchemaTreeNode child in node.Children)
        {
            children.Add(BuildNode(child, sourceKey, isSource, mappings, diagnostics, projections, activeProjection));
        }

        ConditionalMappingDisplayModel mapping = new();

        if (mappings.TryGetValue(path, out ConditionalMappingDisplayModel storedMapping))
        {
            mapping = storedMapping;
        }

        bool isTemplateField = !isSource && node.Kind == SchemaNodeKind.Scalar && !string.IsNullOrWhiteSpace(activeProjection.TargetPath) && path.StartsWith(activeProjection.TargetPath + "[0].", StringComparison.Ordinal);
        string projectionFieldPath = string.Empty;

        if (isTemplateField)
        {
            projectionFieldPath = path[(activeProjection.TargetPath.Length + 4)..];

            if (activeProjection.FieldMappings.TryGetValue(projectionFieldPath, out ConditionalMappingDisplayModel storedFieldMapping))
            {
                mapping = storedFieldMapping;
            }
            else if (activeProjection.FieldExpressions.TryGetValue(projectionFieldPath, out string storedFieldExpression))
            {
                mapping = new ConditionalMappingDisplayModel
                {
                    Expression = storedFieldExpression
                };
            }
        }

        IReadOnlyCollection<string> nodeDiagnostics = [];

        if (diagnostics.TryGetValue(path, out IReadOnlyCollection<string> storedDiagnostics))
        {
            nodeDiagnostics = storedDiagnostics;
        }

        return new SchemaTreeDisplayNode
        {
            Depth = 0,
            Path = path,
            Name = node.Name,
            Kind = node.Kind,
            DataType = node.DataType,
            Children = children,
            IsExpanded = true,
            CanDrag = isSource && (node.Kind == SchemaNodeKind.Scalar || node.Kind == SchemaNodeKind.Array),
            Expression = mapping.Expression,
            MappingMode = mapping.Mode,
            ConditionalConditionExpression = mapping.ConditionExpression,
            ConditionalThenExpression = mapping.ThenExpression,
            ConditionalElseExpression = mapping.ElseExpression,
            Placeholder = CreatePlaceholder(path),
            Diagnostics = nodeDiagnostics,
            IsArrayProjection = isArrayProjection,
            ProjectionSourceExpression = activeProjection.SourceExpression,
            ProjectionAlias = activeProjection.Alias,
            ProjectionAdvancedExpression = activeProjection.AdvancedExpression,
            IsArrayTemplateField = isTemplateField,
            ProjectionTargetPath = activeProjection.TargetPath,
            ProjectionFieldPath = projectionFieldPath
        };
    }

    private static string CreateSourcePath(string sourceKey, string schemaPath)
    {
        if (string.Equals(schemaPath, "$root", StringComparison.Ordinal))
        {
            return "$" + sourceKey;
        }

        return "$" + sourceKey + "." + schemaPath;
    }

    private static string CreatePlaceholder(string targetPath)
    {
        if (string.Equals(targetPath, "$root", StringComparison.Ordinal))
        {
            return string.Empty;
        }

        string normalizedPath = targetPath.Replace("[0]", string.Empty, StringComparison.Ordinal);
        return "$source." + normalizedPath;
    }
}
