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
        IReadOnlyDictionary<string, string> expressions,
        IReadOnlyDictionary<string, IReadOnlyCollection<string>> diagnostics,
        IReadOnlyDictionary<string, ArrayProjectionDisplayModel> projections)
    {
        return BuildNode(root, string.Empty, false, expressions, diagnostics, projections, new ArrayProjectionDisplayModel());
    }

    private static SchemaTreeDisplayNode BuildNode(
        ISchemaTreeNode node,
        string sourceKey,
        bool isSource,
        IReadOnlyDictionary<string, string> expressions)
    {
        return BuildNode(node, sourceKey, isSource, expressions, new Dictionary<string, IReadOnlyCollection<string>>(), new Dictionary<string, ArrayProjectionDisplayModel>(), new ArrayProjectionDisplayModel());
    }

    private static SchemaTreeDisplayNode BuildNode(
        ISchemaTreeNode node,
        string sourceKey,
        bool isSource,
        IReadOnlyDictionary<string, string> expressions,
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
            children.Add(BuildNode(child, sourceKey, isSource, expressions, diagnostics, projections, activeProjection));
        }

        string expression = string.Empty;

        if (expressions.TryGetValue(path, out string storedExpression))
        {
            expression = storedExpression;
        }

        bool isTemplateField = !isSource && node.Kind == SchemaNodeKind.Scalar && !string.IsNullOrWhiteSpace(activeProjection.TargetPath) && path.StartsWith(activeProjection.TargetPath + "[0].", StringComparison.Ordinal);
        string projectionFieldPath = string.Empty;

        if (isTemplateField)
        {
            projectionFieldPath = path[(activeProjection.TargetPath.Length + 4)..];

            if (activeProjection.FieldExpressions.TryGetValue(projectionFieldPath, out string storedFieldExpression))
            {
                expression = storedFieldExpression;
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
            Expression = expression,
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
