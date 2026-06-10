namespace ButterMorph.Navigation;

using System;
using ButterMorph.Abstractions;

/// <summary>
/// Provides structure graph navigation capabilities.
/// </summary>
public sealed class NavigationEngine : INavigationEngine
{
    // Resolves paths once the source graph has been selected from the execution context.
    private readonly IPathResolver _pathResolver;

    /// <summary>
    /// Initializes a new instance of the <see cref="NavigationEngine"/> class.
    /// </summary>
    /// <param name="pathResolver">The path resolver.</param>
    public NavigationEngine(IPathResolver pathResolver)
    {
        if (pathResolver is null)
        {
            throw new InvalidOperationException("A path resolver must be registered before resolving navigation paths.");
        }

        _pathResolver = pathResolver;
    }

    /// <summary>
    /// Selects a node from the execution context using a path.
    /// </summary>
    /// <param name="context">The execution context.</param>
    /// <param name="path">The navigation path.</param>
    /// <returns>The selected structure node.</returns>
    public IStructureNode Select(IExecutionContext context, string path)
    {
        if (string.IsNullOrWhiteSpace(path) || !path.StartsWith("$", StringComparison.Ordinal))
        {
            throw new FormatException("Navigation paths must start with a source alias prefixed by '$'.");
        }

        string normalizedPath = path.Substring(1);
        int separatorIndex = normalizedPath.IndexOf('.', StringComparison.Ordinal);
        string sourceName = normalizedPath;

        if (separatorIndex >= 0)
        {
            sourceName = normalizedPath.Substring(0, separatorIndex);
        }

        if (string.IsNullOrWhiteSpace(sourceName))
        {
            throw new FormatException("Navigation paths must include a source alias after '$'.");
        }

        if (!context.Sources.TryGetValue(sourceName, out IStructureGraph graph))
        {
            throw new KeyNotFoundException($"Source '{sourceName}' was not found in the execution context.");
        }

        if (separatorIndex < 0)
        {
            return graph.Root;
        }

        string relativePath = normalizedPath.Substring(separatorIndex + 1);
        return _pathResolver.Resolve(graph.Root, relativePath);
    }
}
