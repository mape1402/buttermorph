using ButterMorph.Abstractions;

namespace ButterMorph.Core;

/// <summary>
/// Represents a node in the canonical internal structure graph.
/// </summary>
public sealed class StructureNode : IStructureNode
{
    /// <summary>
    /// Gets or sets the node name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the canonical node type.
    /// </summary>
    public StructureNodeType Type { get; set; }

    /// <summary>
    /// Gets the node type name.
    /// </summary>
    public string NodeType => Type.ToString();

    /// <summary>
    /// Gets or sets the node value.
    /// </summary>
    public object Value { get; set; }

    /// <summary>
    /// Gets or sets the child nodes.
    /// </summary>
    public IReadOnlyCollection<IStructureNode> Children { get; set; } = new List<IStructureNode>();
}
