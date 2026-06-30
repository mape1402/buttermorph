namespace ButterMorph.Core;

using ButterMorph.Abstractions;

/// <summary>
/// Represents a transformation expression that contains scalar literal values.
/// </summary>
public sealed class ScalarCollectionLiteralExpression : IScalarCollectionLiteralExpression
{
    /// <summary>
    /// Gets the expression kind.
    /// </summary>
    public TransformationExpressionKind Kind => TransformationExpressionKind.ScalarCollectionLiteral;

    /// <summary>
    /// Gets or sets the scalar literal values.
    /// </summary>
    public IReadOnlyCollection<IScalarValue> Values { get; set; } = [];
}
