using ButterMorph.Abstractions;

namespace ButterMorph.Core;

/// <summary>
/// Represents a scalar value collection argument passed to a DSL function.
/// </summary>
public sealed class ScalarCollectionFunctionArgument : IScalarCollectionFunctionArgument
{
    /// <summary>
    /// Gets the argument value shape.
    /// </summary>
    public FunctionValueKind Kind => FunctionValueKind.ScalarCollection;

    /// <summary>
    /// Gets or sets the scalar value collection argument.
    /// </summary>
    public IReadOnlyCollection<IScalarValue> Values { get; set; } = new List<IScalarValue>();
}
