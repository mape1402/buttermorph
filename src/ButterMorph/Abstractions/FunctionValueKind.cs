namespace ButterMorph.Abstractions;

/// <summary>
/// Defines the value shape accepted or produced by a function.
/// </summary>
public enum FunctionValueKind
{
    /// <summary>
    /// Represents a scalar function value.
    /// </summary>
    Scalar,

    /// <summary>
    /// Represents a collection of scalar function values.
    /// </summary>
    ScalarCollection,

    /// <summary>
    /// Represents a single structure node function value.
    /// </summary>
    StructureNode,

    /// <summary>
    /// Represents a collection of structure node function values.
    /// </summary>
    StructureNodeCollection
}
