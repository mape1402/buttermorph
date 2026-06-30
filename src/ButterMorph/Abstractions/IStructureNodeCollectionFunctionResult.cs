namespace ButterMorph.Abstractions;

/// <summary>
/// Represents a structure node collection result produced by a DSL function.
/// </summary>
public interface IStructureNodeCollectionFunctionResult : IFunctionResult
{
    /// <summary>
    /// Gets the structure node collection result.
    /// </summary>
    IReadOnlyCollection<IStructureNode> Nodes { get; }
}
