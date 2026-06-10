namespace ButterMorph.Abstractions;

/// <summary>
/// Represents a scalar value collection result produced by a DSL function.
/// </summary>
public interface IScalarCollectionFunctionResult : IFunctionResult
{
    /// <summary>
    /// Gets the scalar value collection result.
    /// </summary>
    IReadOnlyCollection<IScalarValue> Values { get; }
}
