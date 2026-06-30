using ButterMorph.Abstractions;

namespace ButterMorph.Core;

/// <summary>
/// Represents a structure node collection result produced by a DSL function.
/// </summary>
public sealed class StructureNodeCollectionFunctionResult : IStructureNodeCollectionFunctionResult
{
    /// <summary>
    /// Gets the result value shape.
    /// </summary>
    public FunctionValueKind Kind => FunctionValueKind.StructureNodeCollection;

    /// <summary>
    /// Gets or sets the structure node collection result.
    /// </summary>
    public IReadOnlyCollection<IStructureNode> Nodes { get; set; } = new List<IStructureNode>();
}
