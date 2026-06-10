namespace ButterMorph.UnitTests;

using ButterMorph.Abstractions;

/// <summary>
/// Verifies ButterMorph engine orchestration behavior.
/// </summary>
public sealed class ButterMorphEngineTests
{
    /// <summary>
    /// Confirms that transformation requests are delegated to the configured engine.
    /// </summary>
    [Fact]
    public void TransformDelegatesToConfiguredTransformationEngine()
    {
        FakeExecutionPipeline pipeline = new();
        TransformationResult expectedResult = new()
        {
            Succeeded = true
        };
        TransformationRequest request = new();
        pipeline.TransformationResult = expectedResult;

        IButterMorphEngine engine = new ButterMorphEngine(pipeline);

        TransformationResult result = engine.Transform(request);

        Assert.Same(expectedResult, result);
        Assert.Same(request, pipeline.LastTransformationRequest);
        Assert.Equal(1, pipeline.TransformationExecutionCount);
    }

    /// <summary>
    /// Confirms that validation requests are delegated to the configured engine.
    /// </summary>
    [Fact]
    public void ValidateDelegatesToConfiguredValidationEngine()
    {
        FakeExecutionPipeline pipeline = new();
        ValidationResult expectedResult = new()
        {
            IsValid = true
        };
        ValidationRequest request = new();
        pipeline.ValidationResult = expectedResult;

        IButterMorphEngine engine = new ButterMorphEngine(pipeline);

        ValidationResult result = engine.Validate(request);

        Assert.Same(expectedResult, result);
        Assert.Same(request, pipeline.LastValidationRequest);
        Assert.Equal(1, pipeline.ValidationExecutionCount);
    }
}
