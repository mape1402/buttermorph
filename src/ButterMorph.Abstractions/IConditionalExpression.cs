namespace ButterMorph.Abstractions;

/// <summary>
/// Represents a transformation expression that selects between two branches.
/// </summary>
public interface IConditionalExpression : ITransformationExpression
{
    /// <summary>
    /// Gets the condition expression.
    /// </summary>
    ITransformationExpression Condition { get; }

    /// <summary>
    /// Gets the expression evaluated when the condition is true.
    /// </summary>
    ITransformationExpression ThenExpression { get; }

    /// <summary>
    /// Gets the expression evaluated when the condition is false.
    /// </summary>
    ITransformationExpression ElseExpression { get; }
}
