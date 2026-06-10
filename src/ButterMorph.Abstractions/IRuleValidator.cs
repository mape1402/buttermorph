namespace ButterMorph.Abstractions;

/// <summary>
/// Defines rule validation behavior.
/// </summary>
public interface IRuleValidator
{
    /// <summary>
    /// Validates a request against validation rules.
    /// </summary>
    /// <param name="request">The validation request.</param>
    /// <returns>The validation result.</returns>
    ValidationResult Validate(ValidationRequest request);
}
