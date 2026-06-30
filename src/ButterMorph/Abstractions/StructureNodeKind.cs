namespace ButterMorph.Abstractions;

/// <summary>
/// Defines the structural shape of a node in the internal graph.
/// </summary>
public enum StructureNodeKind
{
    /// <summary>
    /// Represents an Object node.
    /// </summary>
    Object,

    /// <summary>
    /// Represents an Array node.
    /// </summary>
    Array,

    /// <summary>
    /// Represents a Scalar node.
    /// </summary>
    Scalar
}
