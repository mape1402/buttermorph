namespace ButterMorph.UnitTests;

using ButterMorph.Abstractions;

/// <summary>
/// Provides a test validation engine that records execution.
/// </summary>
internal sealed class FakeValidationEngine : IValidationEngine
{
    /// <summary>
    /// Gets the number of validation executions.
    /// </summary>
    public int ExecutionCount { get; private set; }

    /// <summary>
    /// Gets the last validation request received by the fake engine.
    /// </summary>
    public ValidationRequest LastRequest { get; private set; } = new();

    /// <summary>
    /// Gets or sets the validation result returned by the fake engine.
    /// </summary>
    public ValidationResult Result { get; set; } = new();

    /// <summary>
    /// Executes a fake validation.
    /// </summary>
    /// <param name="request">The validation request.</param>
    /// <returns>The configured validation result.</returns>
    public ValidationResult Validate(ValidationRequest request)
    {
        ExecutionCount++;
        LastRequest = request;
        return Result;
    }
}
