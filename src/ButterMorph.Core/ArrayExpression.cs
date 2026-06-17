namespace ButterMorph.Core;

using ButterMorph.Abstractions;

/// <summary>
/// Represents a transformation expression that builds an ordered node.
/// </summary>
public sealed class ArrayExpression : IArrayExpression
{
    /// <summary>
    /// Gets the expression kind.
    /// </summary>
    public TransformationExpressionKind Kind => TransformationExpressionKind.Array;

    /// <summary>
    /// Gets or sets the item expressions.
    /// </summary>
    public IReadOnlyCollection<ITransformationExpression> Items { get; set; } = [];
}
