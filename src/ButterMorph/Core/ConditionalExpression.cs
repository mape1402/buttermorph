namespace ButterMorph.Core;

using ButterMorph.Abstractions;

/// <summary>
/// Represents a transformation expression that selects between two branches.
/// </summary>
public sealed class ConditionalExpression : IConditionalExpression
{
    /// <summary>
    /// Gets the expression kind.
    /// </summary>
    public TransformationExpressionKind Kind => TransformationExpressionKind.Conditional;

    /// <summary>
    /// Gets or sets the condition expression.
    /// </summary>
    public ITransformationExpression Condition { get; set; } = new ScalarLiteralExpression();

    /// <summary>
    /// Gets or sets the expression evaluated when the condition is true.
    /// </summary>
    public ITransformationExpression ThenExpression { get; set; } = new ScalarLiteralExpression();

    /// <summary>
    /// Gets or sets the expression evaluated when the condition is false.
    /// </summary>
    public ITransformationExpression ElseExpression { get; set; } = new ScalarLiteralExpression();
}
