namespace ButterMorph.Abstractions;

/// <summary>
/// Represents a scalar value collection argument passed to a DSL function.
/// </summary>
public interface IScalarCollectionFunctionArgument : IFunctionArgument
{
    /// <summary>
    /// Gets the scalar value collection argument.
    /// </summary>
    IReadOnlyCollection<IScalarValue> Values { get; }
}
