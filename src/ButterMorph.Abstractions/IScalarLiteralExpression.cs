namespace ButterMorph.Abstractions;

/// <summary>
/// Represents a transformation expression that contains a scalar literal.
/// </summary>
public interface IScalarLiteralExpression : ITransformationExpression
{
    /// <summary>
    /// Gets the scalar literal value.
    /// </summary>
    IScalarValue Value { get; }
}
