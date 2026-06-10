namespace ButterMorph.Navigation;

using System;
using System.Globalization;
using System.Linq;
using ButterMorph.Abstractions;

/// <summary>
/// Resolves navigation paths against structure graph nodes.
/// </summary>
public sealed class PathResolver : IPathResolver
{
    /// <summary>
    /// Resolves a path from a root node.
    /// </summary>
    /// <param name="root">The root node.</param>
    /// <param name="path">The path to resolve.</param>
    /// <returns>The resolved node.</returns>
    public IStructureNode Resolve(IStructureNode root, string path)
    {
        if (string.IsNullOrWhiteSpace(path) || string.Equals(path, "$root", StringComparison.Ordinal))
        {
            return root;
        }

        if (path.StartsWith(".", StringComparison.Ordinal) || path.EndsWith(".", StringComparison.Ordinal) || path.Contains("..", StringComparison.Ordinal))
        {
            throw new FormatException("Path contains an empty segment.");
        }

        IStructureNode current = root;

        foreach (string segment in path.Split('.'))
        {
            current = ResolveSegment(current, segment);
        }

        return current;
    }

    // Resolves a single supported path segment against the current node.
    private static IStructureNode ResolveSegment(IStructureNode current, string segment)
    {
        int bracketIndex = segment.IndexOf('[', StringComparison.Ordinal);

        if (bracketIndex < 0)
        {
            return ResolveChild(current, segment);
        }

        if (!segment.EndsWith("]", StringComparison.Ordinal) || bracketIndex == 0)
        {
            throw new FormatException($"Path segment '{segment}' is not valid.");
        }

        string childName = segment.Substring(0, bracketIndex);
        string indexText = segment.Substring(bracketIndex + 1, segment.Length - bracketIndex - 2);

        if (!int.TryParse(indexText, NumberStyles.None, CultureInfo.InvariantCulture, out int index))
        {
            throw new FormatException($"Array index '{indexText}' is not valid.");
        }

        IStructureNode child = ResolveChild(current, childName);
        return ResolveIndex(child, index);
    }

    // Resolves a named child from the current node.
    private static IStructureNode ResolveChild(IStructureNode current, string childName)
    {
        if (string.IsNullOrWhiteSpace(childName))
        {
            throw new FormatException("Path segment names cannot be empty.");
        }

        if (current.Kind == StructureNodeKind.Scalar)
        {
            throw new InvalidOperationException($"Cannot navigate child '{childName}' from a scalar node.");
        }

        foreach (IStructureNode child in current.Children)
        {
            if (string.Equals(child.Name, childName, StringComparison.Ordinal))
            {
                return child;
            }
        }

        throw new KeyNotFoundException($"Child '{childName}' was not found under node '{current.Name}'.");
    }

    // Resolves an array index from the current node.
    private static IStructureNode ResolveIndex(IStructureNode current, int index)
    {
        if (index < 0)
        {
            throw new FormatException("Array indexes cannot be negative.");
        }

        if (current.Kind != StructureNodeKind.Array)
        {
            throw new InvalidOperationException($"Cannot index node '{current.Name}' because it is not an array.");
        }

        if (index >= current.Children.Count)
        {
            throw new IndexOutOfRangeException($"Index '{index}' is outside the bounds of array node '{current.Name}'.");
        }

        return current.Children.ElementAt(index);
    }
}
