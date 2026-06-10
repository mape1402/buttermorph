using ButterMorph.Abstractions;

namespace ButterMorph.Core;

/// <summary>
/// Represents the canonical internal structure graph.
/// </summary>
public sealed class StructureGraph : IStructureGraph
{
    /// <summary>
    /// Gets or sets the root node of the graph.
    /// </summary>
    public IStructureNode Root { get; set; }

    /// <summary>
    /// Gets or sets all nodes contained by the graph.
    /// </summary>
    public IReadOnlyCollection<IStructureNode> Nodes { get; set; } = new List<IStructureNode>();
}
