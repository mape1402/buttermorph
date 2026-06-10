namespace ButterMorph.Abstractions;

/// <summary>
/// Represents a scalar argument passed to a DSL function.
/// </summary>
public interface IScalarFunctionArgument : IFunctionArgument
{
    /// <summary>
    /// Gets the scalar argument value.
    /// </summary>
    IScalarValue Value { get; }
}
