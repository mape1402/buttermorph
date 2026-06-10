namespace ButterMorph.Abstractions;

/// <summary>
/// Defines a transformation engine.
/// </summary>
public interface ITransformationEngine
{
    /// <summary>
    /// Executes a transformation request.
    /// </summary>
    /// <param name="request">The transformation request.</param>
    /// <returns>The transformation result.</returns>
    TransformationResult Transform(TransformationRequest request);
}
