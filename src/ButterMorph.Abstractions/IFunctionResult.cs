namespace ButterMorph.Abstractions;

/// <summary>
/// Represents a typed result produced by a DSL function.
/// </summary>
public interface IFunctionResult
{
    /// <summary>
    /// Gets the result value shape.
    /// </summary>
    FunctionValueKind Kind { get; }
}
