using ButterMorph.Abstractions;

namespace ButterMorph.Core;

/// <summary>
/// Represents a scalar argument passed to a DSL function.
/// </summary>
public sealed class ScalarFunctionArgument : IScalarFunctionArgument
{
    /// <summary>
    /// Gets the argument value shape.
    /// </summary>
    public FunctionValueKind Kind => FunctionValueKind.Scalar;

    /// <summary>
    /// Gets or sets the scalar argument value.
    /// </summary>
    public IScalarValue Value { get; set; }
}
