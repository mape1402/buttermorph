namespace ButterMorph.Abstractions;

/// <summary>
/// Represents a scalar node in the internal structure graph.
/// </summary>
public interface IScalarStructureNode : IStructureNode
{
    /// <summary>
    /// Gets the scalar value held by the node.
    /// </summary>
    IScalarValue Value { get; }
}
