namespace ButterMorph.Abstractions;

/// <summary>
/// Defines schema validation behavior.
/// </summary>
public interface ISchemaValidator
{
    /// <summary>
    /// Validates a request against schema rules.
    /// </summary>
    /// <param name="request">The validation request.</param>
    /// <returns>The validation result.</returns>
    ValidationResult Validate(ValidationRequest request);
}
