namespace ButterMorph.Abstractions;

/// <summary>
/// Defines the structural shape of a schema node.
/// </summary>
public enum SchemaNodeKind
{
    /// <summary>
    /// Represents a schema node with named children.
    /// </summary>
    Object,

    /// <summary>
    /// Represents a schema node with ordered item children.
    /// </summary>
    Array,

    /// <summary>
    /// Represents a schema node with scalar data.
    /// </summary>
    Scalar
}
