using ButterMorph.Abstractions;

namespace ButterMorph.Core;

/// <summary>
/// Represents a single structure node argument passed to a DSL function.
/// </summary>
public sealed class StructureNodeFunctionArgument : IStructureNodeFunctionArgument
{
    /// <summary>
    /// Gets the argument value shape.
    /// </summary>
    public FunctionValueKind Kind => FunctionValueKind.StructureNode;

    /// <summary>
    /// Gets or sets the structure node argument.
    /// </summary>
    public IStructureNode Node { get; set; }
}
