namespace ButterMorph.Execution;

using System;
using ButterMorph.Abstractions;

/// <summary>
/// Coordinates the high-level execution pipeline.
/// </summary>
public sealed class ExecutionPipeline : IExecutionPipeline
{
    // Holds the configured transformation runtime while keeping transformation logic outside the pipeline.
    private readonly ITransformationEngine _transformationEngine;

    // Holds the configured validation runtime while keeping validation logic outside the pipeline.
    private readonly IValidationEngine _validationEngine;

    /// <summary>
    /// Initializes a new instance of the <see cref="ExecutionPipeline"/> class.
    /// </summary>
    /// <param name="transformationEngine">The transformation engine.</param>
    /// <param name="validationEngine">The validation engine.</param>
    public ExecutionPipeline(ITransformationEngine transformationEngine, IValidationEngine validationEngine)
    {
        if (transformationEngine is null)
        {
            throw new InvalidOperationException("A transformation engine must be registered before resolving the execution pipeline.");
        }

        if (validationEngine is null)
        {
            throw new InvalidOperationException("A validation engine must be registered before resolving the execution pipeline.");
        }

        _transformationEngine = transformationEngine;
        _validationEngine = validationEngine;
    }

    /// <summary>
    /// Executes a transformation request.
    /// </summary>
    /// <param name="request">The transformation request.</param>
    /// <returns>The transformation result.</returns>
    public TransformationResult Transform(TransformationRequest request)
    {
        return _transformationEngine.Transform(request);
    }

    /// <summary>
    /// Executes a validation request.
    /// </summary>
    /// <param name="request">The validation request.</param>
    /// <returns>The validation result.</returns>
    public ValidationResult Validate(ValidationRequest request)
    {
        return _validationEngine.Validate(request);
    }
}
