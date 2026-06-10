using System.Collections.Generic;

namespace ButterMorph.Abstractions;

/// <summary>
/// Represents a format-independent structure node.
/// </summary>
public interface IStructureNode
{
    /// <summary>
    /// Gets the node name.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Gets the node type name.
    /// </summary>
    string NodeType { get; }

    /// <summary>
    /// Gets the node value.
    /// </summary>
    object Value { get; }

    /// <summary>
    /// Gets the child nodes.
    /// </summary>
    IReadOnlyCollection<IStructureNode> Children { get; }
}
