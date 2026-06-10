namespace ButterMorph.Abstractions;

/// <summary>
/// Defines a validation engine.
/// </summary>
public interface IValidationEngine
{
    /// <summary>
    /// Executes a validation request.
    /// </summary>
    /// <param name="request">The validation request.</param>
    /// <returns>The validation result.</returns>
    ValidationResult Validate(ValidationRequest request);
}
