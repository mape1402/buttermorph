namespace ButterMorph.Abstractions;

/// <summary>
/// Defines path resolution behavior for structure nodes.
/// </summary>
public interface IPathResolver
{
    /// <summary>
    /// Resolves a path from a root node.
    /// </summary>
    /// <param name="root">The root node.</param>
    /// <param name="path">The path to resolve.</param>
    /// <returns>The resolved node.</returns>
    IStructureNode Resolve(IStructureNode root, string path);
}
