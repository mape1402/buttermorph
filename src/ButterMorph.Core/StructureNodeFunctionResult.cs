using ButterMorph.Abstractions;

namespace ButterMorph.Core;

/// <summary>
/// Represents a single structure node result produced by a DSL function.
/// </summary>
public sealed class StructureNodeFunctionResult : IStructureNodeFunctionResult
{
    /// <summary>
    /// Gets the result value shape.
    /// </summary>
    public FunctionValueKind Kind => FunctionValueKind.StructureNode;

    /// <summary>
    /// Gets or sets the structure node result.
    /// </summary>
    public IStructureNode Node { get; set; }
}
