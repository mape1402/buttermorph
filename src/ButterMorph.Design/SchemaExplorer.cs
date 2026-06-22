namespace ButterMorph.Design;

using ButterMorph.Abstractions;

/// <summary>
/// Creates UI-ready schema trees.
/// </summary>
public sealed class SchemaExplorer : ISchemaExplorer
{
    /// <summary>
    /// Explores a structure schema.
    /// </summary>
    /// <param name="schema">The structure schema.</param>
    /// <returns>The schema tree root.</returns>
    public ISchemaTreeNode Explore(IStructureSchema schema)
    {
        if (string.IsNullOrWhiteSpace(schema.Key))
        {
            throw new InvalidOperationException("Schema key is required before exploring a schema.");
        }

        if (string.IsNullOrWhiteSpace(schema.Name))
        {
            throw new InvalidOperationException("Schema name is required before exploring a schema.");
        }

        if (string.IsNullOrWhiteSpace(schema.Version))
        {
            throw new InvalidOperationException("Schema version is required before exploring a schema.");
        }

        return CreateNode(schema.Root, string.Empty);
    }

    // Creates a schema tree node and its children.
    private static ISchemaTreeNode CreateNode(ISchemaNode node, string parentPath)
    {
        string path = CreatePath(node, parentPath);
        List<ISchemaTreeNode> children = [];

        foreach (ISchemaNode child in node.Children)
        {
            children.Add(CreateNode(child, path));
        }

        return new SchemaTreeNode
        {
            Name = node.Name,
            Path = path,
            Kind = node.Kind,
            DataType = node.DataType,
            IsRequired = node.IsRequired,
            Metadata = node.Metadata,
            Children = children
        };
    }

    // Creates a UI path for a schema node.
    private static string CreatePath(ISchemaNode node, string parentPath)
    {
        if (string.Equals(node.Name, "$root", StringComparison.Ordinal))
        {
            return "$root";
        }

        if (string.Equals(node.Name, "$item", StringComparison.Ordinal))
        {
            return $"{parentPath}[0]";
        }

        if (string.IsNullOrWhiteSpace(parentPath) || string.Equals(parentPath, "$root", StringComparison.Ordinal))
        {
            return node.Name;
        }

        return $"{parentPath}.{node.Name}";
    }
}
