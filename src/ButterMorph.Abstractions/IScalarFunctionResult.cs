namespace ButterMorph.Abstractions;

/// <summary>
/// Represents a scalar result produced by a DSL function.
/// </summary>
public interface IScalarFunctionResult : IFunctionResult
{
    /// <summary>
    /// Gets the scalar result value.
    /// </summary>
    IScalarValue Value { get; }
}
