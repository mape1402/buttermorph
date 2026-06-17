namespace ButterMorph.Design;

using ButterMorph.Abstractions;

/// <summary>
/// Creates UI-ready schema trees.
/// </summary>
public interface ISchemaExplorer
{
    /// <summary>
    /// Explores a structure schema.
    /// </summary>
    /// <param name="schema">The structure schema.</param>
    /// <returns>The schema tree root.</returns>
    ISchemaTreeNode Explore(IStructureSchema schema);
}
