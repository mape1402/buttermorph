namespace ButterMorph.Abstractions;

/// <summary>
/// Represents a transformation expression that contains scalar literal values.
/// </summary>
public interface IScalarCollectionLiteralExpression : ITransformationExpression
{
    /// <summary>
    /// Gets the scalar literal values.
    /// </summary>
    IReadOnlyCollection<IScalarValue> Values { get; }
}
