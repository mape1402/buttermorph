namespace ButterMorph.Core;

/// <summary>
/// Defines the canonical node types used by the internal structure graph.
/// </summary>
public enum StructureNodeType
{
    /// <summary>
    /// Represents an object node.
    /// </summary>
    Object,

    /// <summary>
    /// Represents an array node.
    /// </summary>
    Array,

    /// <summary>
    /// Represents a property node.
    /// </summary>
    Property,

    /// <summary>
    /// Represents a value node.
    /// </summary>
    Value,

    /// <summary>
    /// Represents a null node.
    /// </summary>
    Null
}
