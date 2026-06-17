namespace ButterMorph.Abstractions;

/// <summary>
/// Represents an expression used as a transformation source.
/// </summary>
public interface ITransformationExpression
{
    /// <summary>
    /// Gets the expression kind.
    /// </summary>
    TransformationExpressionKind Kind { get; }
}
