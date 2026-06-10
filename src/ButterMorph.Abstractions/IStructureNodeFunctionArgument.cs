namespace ButterMorph.Abstractions;

/// <summary>
/// Represents a single structure node argument passed to a DSL function.
/// </summary>
public interface IStructureNodeFunctionArgument : IFunctionArgument
{
    /// <summary>
    /// Gets the structure node argument.
    /// </summary>
    IStructureNode Node { get; }
}
