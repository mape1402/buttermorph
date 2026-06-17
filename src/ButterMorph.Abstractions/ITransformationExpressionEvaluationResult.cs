namespace ButterMorph.Abstractions;

/// <summary>
/// Represents the result of evaluating a transformation expression.
/// </summary>
public interface ITransformationExpressionEvaluationResult
{
    /// <summary>
    /// Gets a value indicating whether expression evaluation succeeded.
    /// </summary>
    bool Succeeded { get; }

    /// <summary>
    /// Gets the function-shaped evaluation result.
    /// </summary>
    IFunctionResult Result { get; }

    /// <summary>
    /// Gets diagnostics produced during expression evaluation.
    /// </summary>
    IReadOnlyCollection<DiagnosticEntry> Diagnostics { get; }
}
