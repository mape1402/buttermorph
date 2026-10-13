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
            Description = ResolveDescription(node.Metadata),
            DataType = node.DataType,
            DisplayType = ResolveDisplayType(node.Metadata, node.DataType, node.Kind, node.Name)
        });

        foreach (ISchemaTreeNode child in node.Children)
        {
            AddNode(child, depth + 1, nodes);
        }
    }

    private static string ResolveDescription(IReadOnlyDictionary<string, string> metadata)
    {
        if (metadata.TryGetValue("description", out string description) && !string.IsNullOrWhiteSpace(description))
        {
            return description.Trim();
        }

        return string.Empty;
    }

    private static string ResolveDisplayType(IReadOnlyDictionary<string, string> metadata, string dataType, ButterMorph.Abstractions.SchemaNodeKind kind, string name)
    {
        if (metadata.TryGetValue("$ref", out string reference) && !string.IsNullOrWhiteSpace(reference))
        {
            string label = reference.Trim();
            int slashIndex = label.LastIndexOf('/');
            if (slashIndex >= 0 && slashIndex < label.Length - 1)
            {
                label = label[(slashIndex + 1)..];
            }

            int atIndex = label.IndexOf('@', StringComparison.Ordinal);
            if (atIndex > 0)
            {
                label = label[..atIndex];
            }

            label = Uri.UnescapeDataString(label).Trim();
            if (!string.IsNullOrWhiteSpace(label) && !Guid.TryParse(label, out _))
            {
                return label;
            }

            string fallback = ResolveReferenceFallbackName(name);
            if (!string.IsNullOrWhiteSpace(fallback))
            {
                return fallback;
            }
        }

        foreach (string key in new[] { "typeName", "typeKey", "typeId" })
        {
            if (metadata.TryGetValue(key, out string value) && !string.IsNullOrWhiteSpace(value) && !Guid.TryParse(value, out _))
            {
                return value.Trim();
            }
        }

        return string.IsNullOrWhiteSpace(dataType) ? kind.ToString().ToLowerInvariant() : dataType.Trim();
    }

    private static string ResolveReferenceFallbackName(string name)
    {
        if (string.IsNullOrWhiteSpace(name) ||
            string.Equals(name, "$root", StringComparison.Ordinal) ||
            string.Equals(name, "$item", StringComparison.Ordinal))
        {
            return string.Empty;
        }

        return name.Trim();
    }
}
