namespace ButterMorph.Core;

using ButterMorph.Abstractions;

/// <summary>
/// Represents a transformation expression that contains a scalar literal.
/// </summary>
public sealed class ScalarLiteralExpression : IScalarLiteralExpression
{
    /// <summary>
    /// Gets the expression kind.
    /// </summary>
    public TransformationExpressionKind Kind => TransformationExpressionKind.ScalarLiteral;

    /// <summary>
    /// Gets or sets the scalar literal value.
    /// </summary>
    public IScalarValue Value { get; set; } = new ScalarValue();
}
