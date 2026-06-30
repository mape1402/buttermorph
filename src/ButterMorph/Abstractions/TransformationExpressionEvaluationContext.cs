namespace ButterMorph.Abstractions;

/// <summary>
/// Represents context used while evaluating a transformation expression.
/// </summary>
public sealed class TransformationExpressionEvaluationContext
{
    /// <summary>
    /// Gets or sets the shared execution context.
    /// </summary>
    public IExecutionContext ExecutionContext { get; set; }

    /// <summary>
    /// Gets or sets the expression to evaluate.
    /// </summary>
    public ITransformationExpression Expression { get; set; }

    /// <summary>
    /// Gets or sets scoped alias nodes available to the expression.
    /// </summary>
    public IReadOnlyDictionary<string, IStructureNode> Aliases { get; set; } = new Dictionary<string, IStructureNode>();
}
