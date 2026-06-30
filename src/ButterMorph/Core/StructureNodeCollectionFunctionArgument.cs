using ButterMorph.Abstractions;

namespace ButterMorph.Core;

/// <summary>
/// Represents a structure node collection argument passed to a DSL function.
/// </summary>
public sealed class StructureNodeCollectionFunctionArgument : IStructureNodeCollectionFunctionArgument
{
    /// <summary>
    /// Gets the argument value shape.
    /// </summary>
    public FunctionValueKind Kind => FunctionValueKind.StructureNodeCollection;

    /// <summary>
    /// Gets or sets the structure node collection argument.
    /// </summary>
    public IReadOnlyCollection<IStructureNode> Nodes { get; set; } = new List<IStructureNode>();
}
