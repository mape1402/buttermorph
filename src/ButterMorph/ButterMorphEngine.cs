namespace ButterMorph;

using ButterMorph.Abstractions;

/// <summary>
/// Provides the public execution entry point for ButterMorph.
/// </summary>
public sealed class ButterMorphEngine : IButterMorphEngine
{
    // Owns the abstract graph-level execution pipeline used by the public facade.
    private readonly IExecutionPipeline _executionPipeline;

    /// <summary>
    /// Initializes a new instance of the <see cref="ButterMorphEngine"/> class.
    /// </summary>
    /// <param name="executionPipeline">The execution pipeline.</param>
    public ButterMorphEngine(IExecutionPipeline executionPipeline)
    {
        _executionPipeline = executionPipeline;
    }

    /// <summary>
    /// Executes a transformation request.
    /// </summary>
    /// <param name="request">The transformation request.</param>
    /// <returns>The transformation result.</returns>
    public TransformationResult Transform(TransformationRequest request)
    {
        return _executionPipeline.Transform(request);
    }

    /// <summary>
    /// Executes a validation request.
    /// </summary>
    /// <param name="request">The validation request.</param>
    /// <returns>The validation result.</returns>
    public ValidationResult Validate(ValidationRequest request)
    {
        return _executionPipeline.Validate(request);
    }
}
