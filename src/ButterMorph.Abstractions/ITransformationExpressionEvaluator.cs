namespace ButterMorph.Abstractions;

/// <summary>
/// Defines evaluation behavior for transformation expressions.
/// </summary>
public interface ITransformationExpressionEvaluator
{
    /// <summary>
    /// Evaluates a transformation expression.
    /// </summary>
    /// <param name="context">The expression evaluation context.</param>
    /// <returns>The expression evaluation result.</returns>
    ITransformationExpressionEvaluationResult Evaluate(TransformationExpressionEvaluationContext context);
}
