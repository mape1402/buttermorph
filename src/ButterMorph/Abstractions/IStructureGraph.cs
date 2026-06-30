namespace ButterMorph.Abstractions;

/// <summary>
/// Represents a format-independent structure graph.
/// </summary>
public interface IStructureGraph
{
    /// <summary>
    /// Gets the root node of the structure graph.
    /// </summary>
    IStructureNode Root { get; }

    /// <summary>
    /// Gets all nodes contained by the structure graph.
    /// </summary>
    IReadOnlyCollection<IStructureNode> Nodes { get; }
}
