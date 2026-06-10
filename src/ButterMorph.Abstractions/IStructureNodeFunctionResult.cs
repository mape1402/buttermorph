namespace ButterMorph.Abstractions;

/// <summary>
/// Represents a single structure node result produced by a DSL function.
/// </summary>
public interface IStructureNodeFunctionResult : IFunctionResult
{
    /// <summary>
    /// Gets the structure node result.
    /// </summary>
    IStructureNode Node { get; }
}
