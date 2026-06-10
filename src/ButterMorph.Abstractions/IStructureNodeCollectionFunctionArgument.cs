namespace ButterMorph.Abstractions;

/// <summary>
/// Represents a structure node collection argument passed to a DSL function.
/// </summary>
public interface IStructureNodeCollectionFunctionArgument : IFunctionArgument
{
    /// <summary>
    /// Gets the structure node collection argument.
    /// </summary>
    IReadOnlyCollection<IStructureNode> Nodes { get; }
}
