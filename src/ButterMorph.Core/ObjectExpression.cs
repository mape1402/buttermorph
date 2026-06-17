namespace ButterMorph.Core;

using ButterMorph.Abstractions;

/// <summary>
/// Represents a transformation expression that builds a map-shaped node.
/// </summary>
public sealed class ObjectExpression : IObjectExpression
{
    /// <summary>
    /// Gets the expression kind.
    /// </summary>
    public TransformationExpressionKind Kind => TransformationExpressionKind.Object;

    /// <summary>
    /// Gets or sets the property expressions.
    /// </summary>
    public IReadOnlyCollection<IObjectPropertyExpression> Properties { get; set; } = [];
}
