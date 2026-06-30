using ButterMorph.Abstractions;

namespace ButterMorph.Core;

/// <summary>
/// Represents a scalar node in the canonical internal structure graph.
/// </summary>
public sealed class ScalarStructureNode : IScalarStructureNode
{
    /// <summary>
    /// Gets or sets the node name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets the structural node kind.
    /// </summary>
    public StructureNodeKind Kind => StructureNodeKind.Scalar;

    /// <summary>
    /// Gets or sets the scalar value held by the node.
    /// </summary>
    public IScalarValue Value { get; set; }

    /// <summary>
    /// Gets or sets the child nodes.
    /// </summary>
    public IReadOnlyCollection<IStructureNode> Children { get; set; } = new List<IStructureNode>();
}
