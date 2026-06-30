namespace ButterMorph.Abstractions;

/// <summary>
/// Represents a typed argument passed to a DSL function.
/// </summary>
public interface IFunctionArgument
{
    /// <summary>
    /// Gets the argument value shape.
    /// </summary>
    FunctionValueKind Kind { get; }
}
