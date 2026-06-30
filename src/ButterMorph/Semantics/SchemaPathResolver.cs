namespace ButterMorph.Semantics;

using ButterMorph.Abstractions;

/// <summary>
/// Resolves paths against structure schemas.
/// </summary>
public sealed class SchemaPathResolver : ISchemaPathResolver
{
    /// <summary>
    /// Resolves a path relative to a schema root node.
    /// </summary>
    /// <param name="root">The schema root node.</param>
    /// <param name="path">The schema path.</param>
    /// <returns>The resolved schema node.</returns>
    public ISchemaNode Resolve(ISchemaNode root, string path)
    {
        if (string.IsNullOrWhiteSpace(path) || string.Equals(path, "$root", StringComparison.Ordinal))
        {
            return root;
        }

        ISchemaNode current = root;
        string[] segments = path.Split('.', StringSplitOptions.RemoveEmptyEntries);

        foreach (string segment in segments)
        {
            current = ResolveSegment(current, segment);
        }

        return current;
    }

    // Resolves one schema path segment and any array index marker.
    private static ISchemaNode ResolveSegment(ISchemaNode current, string segment)
    {
        if (string.IsNullOrWhiteSpace(segment))
        {
            throw new FormatException("Schema path contains an empty segment.");
        }

        string name = segment;
        bool hasIndex = false;
        int bracketIndex = segment.IndexOf('[', StringComparison.Ordinal);

        if (bracketIndex >= 0)
        {
            if (!segment.EndsWith("]", StringComparison.Ordinal))
            {
                throw new FormatException($"Schema path segment '{segment}' has invalid index syntax.");
            }

            name = segment[..bracketIndex];
            string indexText = segment[(bracketIndex + 1)..^1];

            if (!int.TryParse(indexText, out int index) || index < 0)
            {
                throw new FormatException($"Schema path segment '{segment}' has invalid index syntax.");
            }

            hasIndex = true;
        }

        ISchemaNode child = ResolveChild(current, name);

        if (!hasIndex)
        {
            return child;
        }

        if (child.Kind != SchemaNodeKind.Array)
        {
            throw new InvalidOperationException($"Schema node '{child.Name}' is not array-shaped.");
        }

        return ResolveArrayItem(child);
    }

    // Resolves a named child from a map-shaped schema node.
    private static ISchemaNode ResolveChild(ISchemaNode current, string name)
    {
        if (current.Kind != SchemaNodeKind.Object)
        {
            throw new InvalidOperationException($"Schema node '{current.Name}' cannot be traversed by name.");
        }

        foreach (ISchemaNode child in current.Children)
        {
            if (string.Equals(child.Name, name, StringComparison.Ordinal))
            {
                return child;
            }
        }

        throw new KeyNotFoundException($"Schema child '{name}' was not found under '{current.Name}'.");
    }

    // Resolves the conventional schema item node for an array.
    private static ISchemaNode ResolveArrayItem(ISchemaNode arrayNode)
    {
        foreach (ISchemaNode child in arrayNode.Children)
        {
            if (string.Equals(child.Name, "$item", StringComparison.Ordinal))
            {
                return child;
            }
        }

        foreach (ISchemaNode child in arrayNode.Children)
        {
            return child;
        }

        throw new KeyNotFoundException($"Schema array '{arrayNode.Name}' does not define an item schema.");
    }
}
