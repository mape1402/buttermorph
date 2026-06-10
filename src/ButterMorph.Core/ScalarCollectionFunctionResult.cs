using ButterMorph.Abstractions;

namespace ButterMorph.Core;

/// <summary>
/// Represents a scalar value collection result produced by a DSL function.
/// </summary>
public sealed class ScalarCollectionFunctionResult : IScalarCollectionFunctionResult
{
    /// <summary>
    /// Gets the result value shape.
    /// </summary>
    public FunctionValueKind Kind => FunctionValueKind.ScalarCollection;

    /// <summary>
    /// Gets or sets the scalar value collection result.
    /// </summary>
    public IReadOnlyCollection<IScalarValue> Values { get; set; } = new List<IScalarValue>();
}
