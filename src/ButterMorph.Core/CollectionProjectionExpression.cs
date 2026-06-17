namespace ButterMorph.Core;

using ButterMorph.Abstractions;

/// <summary>
/// Represents a transformation expression that projects a source collection.
/// </summary>
public sealed class CollectionProjectionExpression : ICollectionProjectionExpression
{
    /// <summary>
    /// Gets the expression kind.
    /// </summary>
    public TransformationExpressionKind Kind => TransformationExpressionKind.CollectionProjection;

    /// <summary>
    /// Gets or sets the source collection expression.
    /// </summary>
    public ITransformationExpression SourceExpression { get; set; } = new PathExpression();

    /// <summary>
    /// Gets or sets the alias used for each item while evaluating the body.
    /// </summary>
    public string ItemAlias { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the body expression evaluated for each item.
    /// </summary>
    public ITransformationExpression BodyExpression { get; set; } = new PathExpression();
}
