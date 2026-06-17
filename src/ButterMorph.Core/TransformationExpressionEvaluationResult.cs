namespace ButterMorph.Core;

using ButterMorph.Abstractions;

/// <summary>
/// Represents the result of evaluating a transformation expression.
/// </summary>
public sealed class TransformationExpressionEvaluationResult : ITransformationExpressionEvaluationResult
{
    /// <summary>
    /// Gets or sets a value indicating whether expression evaluation succeeded.
    /// </summary>
    public bool Succeeded { get; set; }

    /// <summary>
    /// Gets or sets the function-shaped evaluation result.
    /// </summary>
    public IFunctionResult Result { get; set; } = new ScalarFunctionResult
    {
        Value = new ScalarValue()
    };

    /// <summary>
    /// Gets or sets diagnostics produced during expression evaluation.
    /// </summary>
    public IReadOnlyCollection<DiagnosticEntry> Diagnostics { get; set; } = [];
}
