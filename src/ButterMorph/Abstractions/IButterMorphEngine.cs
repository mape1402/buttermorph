namespace ButterMorph.Abstractions;

/// <summary>
/// Defines the public ButterMorph execution entry point.
/// </summary>
public interface IButterMorphEngine
{
    /// <summary>
    /// Executes a transformation request.
    /// </summary>
    /// <param name="request">The transformation request.</param>
    /// <returns>The transformation result.</returns>
    TransformationResult Transform(TransformationRequest request);

    /// <summary>
    /// Executes a validation request.
    /// </summary>
    /// <param name="request">The validation request.</param>
    /// <returns>The validation result.</returns>
    ValidationResult Validate(ValidationRequest request);
}
