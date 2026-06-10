namespace ButterMorph.UnitTests;

using ButterMorph.Abstractions;

/// <summary>
/// Provides a test transformation engine that records execution.
/// </summary>
internal sealed class FakeTransformationEngine : ITransformationEngine
{
    /// <summary>
    /// Gets the number of transformation executions.
    /// </summary>
    public int ExecutionCount { get; private set; }

    /// <summary>
    /// Gets the last transformation request received by the fake engine.
    /// </summary>
    public TransformationRequest LastRequest { get; private set; } = new();

    /// <summary>
    /// Gets or sets the transformation result returned by the fake engine.
    /// </summary>
    public TransformationResult Result { get; set; } = new();

    /// <summary>
    /// Executes a fake transformation.
    /// </summary>
    /// <param name="request">The transformation request.</param>
    /// <returns>The configured transformation result.</returns>
    public TransformationResult Transform(TransformationRequest request)
    {
        ExecutionCount++;
        LastRequest = request;
        return Result;
    }
}
