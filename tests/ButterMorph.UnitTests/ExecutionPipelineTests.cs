namespace ButterMorph.UnitTests;

using ButterMorph.Abstractions;
using ButterMorph.Execution;

/// <summary>
/// Verifies execution pipeline behavior.
/// </summary>
public sealed class ExecutionPipelineTests
{
    /// <summary>
    /// Confirms that transformation requests are delegated to the configured transformation engine.
    /// </summary>
    [Fact]
    public void TransformDelegatesToTransformationEngine()
    {
        FakeTransformationEngine transformationEngine = new();
        TransformationResult expectedResult = new()
        {
            Succeeded = true
        };
        TransformationRequest request = new();
        transformationEngine.Result = expectedResult;
        IExecutionPipeline pipeline = new ExecutionPipeline(transformationEngine, new FakeValidationEngine());

        TransformationResult result = pipeline.Transform(request);

        Assert.Same(expectedResult, result);
        Assert.Same(request, transformationEngine.LastRequest);
        Assert.Equal(1, transformationEngine.ExecutionCount);
    }

    /// <summary>
    /// Confirms that validation requests are delegated to the configured validation engine.
    /// </summary>
    [Fact]
    public void ValidateDelegatesToValidationEngine()
    {
        FakeValidationEngine validationEngine = new();
        ValidationResult expectedResult = new()
        {
            IsValid = true
        };
        ValidationRequest request = new();
        validationEngine.Result = expectedResult;
        IExecutionPipeline pipeline = new ExecutionPipeline(new FakeTransformationEngine(), validationEngine);

        ValidationResult result = pipeline.Validate(request);

        Assert.Same(expectedResult, result);
        Assert.Same(request, validationEngine.LastRequest);
        Assert.Equal(1, validationEngine.ExecutionCount);
    }
}
