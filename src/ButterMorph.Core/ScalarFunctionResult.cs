using ButterMorph.Abstractions;

namespace ButterMorph.Core;

/// <summary>
/// Represents a scalar result produced by a DSL function.
/// </summary>
public sealed class ScalarFunctionResult : IScalarFunctionResult
{
    /// <summary>
    /// Gets the result value shape.
    /// </summary>
    public FunctionValueKind Kind => FunctionValueKind.Scalar;

    /// <summary>
    /// Gets or sets the scalar result value.
    /// </summary>
    public IScalarValue Value { get; set; }
}
