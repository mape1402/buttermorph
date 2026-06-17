namespace ButterMorph.Web.Razor;

using ButterMorph.Design;

// Flattens schema trees for simple Razor table rendering.
internal static class SchemaTreeFlattener
{
    internal static IReadOnlyCollection<SchemaTreeDisplayNode> Flatten(ISchemaTreeNode root)
    {
        List<SchemaTreeDisplayNode> nodes = [];
        AddNode(root, 0, nodes);

        return nodes;
    }

    private static void AddNode(ISchemaTreeNode node, int depth, List<SchemaTreeDisplayNode> nodes)
    {
        nodes.Add(new SchemaTreeDisplayNode
        {
            Depth = depth,
            Path = node.Path,
            Name = node.Name,
            Kind = node.Kind,
            DataType = node.DataType
        });

        foreach (ISchemaTreeNode child in node.Children)
        {
            AddNode(child, depth + 1, nodes);
        }
    }
}
