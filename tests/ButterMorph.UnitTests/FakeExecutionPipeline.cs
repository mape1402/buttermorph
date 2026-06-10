namespace ButterMorph.UnitTests;

using ButterMorph.Abstractions;

/// <summary>
/// Provides a test execution pipeline that records execution.
/// </summary>
internal sealed class FakeExecutionPipeline : IExecutionPipeline
{
    /// <summary>
    /// Gets the number of transformation executions.
    /// </summary>
    public int TransformationExecutionCount { get; private set; }

    /// <summary>
    /// Gets the number of validation executions.
    /// </summary>
    public int ValidationExecutionCount { get; private set; }

    /// <summary>
    /// Gets the last transformation request received by the fake pipeline.
    /// </summary>
    public TransformationRequest LastTransformationRequest { get; private set; } = new();

    /// <summary>
    /// Gets the last validation request received by the fake pipeline.
    /// </summary>
    public ValidationRequest LastValidationRequest { get; private set; } = new();

    /// <summary>
    /// Gets or sets the transformation result returned by the fake pipeline.
    /// </summary>
    public TransformationResult TransformationResult { get; set; } = new();

    /// <summary>
    /// Gets or sets the validation result returned by the fake pipeline.
    /// </summary>
    public ValidationResult ValidationResult { get; set; } = new();

    /// <summary>
    /// Executes a fake transformation.
    /// </summary>
    /// <param name="request">The transformation request.</param>
    /// <returns>The configured transformation result.</returns>
    public TransformationResult Transform(TransformationRequest request)
    {
        TransformationExecutionCount++;
        LastTransformationRequest = request;
        return TransformationResult;
    }

    /// <summary>
    /// Executes a fake validation.
    /// </summary>
    /// <param name="request">The validation request.</param>
    /// <returns>The configured validation result.</returns>
    public ValidationResult Validate(ValidationRequest request)
    {
        ValidationExecutionCount++;
        LastValidationRequest = request;
        return ValidationResult;
    }
}
