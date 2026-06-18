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
        IReadOnlyDictionary<string, IReadOnlyCollection<string>> diagnostics)
    {
        return BuildNode(root, string.Empty, false, expressions, diagnostics);
    }

    private static SchemaTreeDisplayNode BuildNode(
        ISchemaTreeNode node,
        string sourceKey,
        bool isSource,
        IReadOnlyDictionary<string, string> expressions)
    {
        return BuildNode(node, sourceKey, isSource, expressions, new Dictionary<string, IReadOnlyCollection<string>>());
    }

    private static SchemaTreeDisplayNode BuildNode(
        ISchemaTreeNode node,
        string sourceKey,
        bool isSource,
        IReadOnlyDictionary<string, string> expressions,
        IReadOnlyDictionary<string, IReadOnlyCollection<string>> diagnostics)
    {
        string path = node.Path;

        if (isSource)
        {
            path = CreateSourcePath(sourceKey, node.Path);
        }

        List<SchemaTreeDisplayNode> children = [];

        foreach (ISchemaTreeNode child in node.Children)
        {
            children.Add(BuildNode(child, sourceKey, isSource, expressions, diagnostics));
        }

        string expression = string.Empty;

        if (expressions.TryGetValue(path, out string storedExpression))
        {
            expression = storedExpression;
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
            CanDrag = isSource && node.Kind == SchemaNodeKind.Scalar,
            Expression = expression,
            Placeholder = CreatePlaceholder(path),
            Diagnostics = nodeDiagnostics
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
