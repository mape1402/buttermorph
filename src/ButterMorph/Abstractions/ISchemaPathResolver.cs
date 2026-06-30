namespace ButterMorph.Abstractions;

/// <summary>
/// Resolves paths against structure schemas.
/// </summary>
public interface ISchemaPathResolver
{
    /// <summary>
    /// Resolves a path relative to a schema root node.
    /// </summary>
    /// <param name="root">The schema root node.</param>
    /// <param name="path">The schema path.</param>
    /// <returns>The resolved schema node.</returns>
    ISchemaNode Resolve(ISchemaNode root, string path);
}
