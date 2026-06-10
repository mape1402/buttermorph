namespace ButterMorph.Abstractions;

/// <summary>
/// Defines registration and lookup behavior for validation rule handlers.
/// </summary>
public interface IValidationRuleRegistry
{
    /// <summary>
    /// Registers a validation rule handler using a unique key.
    /// </summary>
    /// <param name="key">The unique rule key.</param>
    /// <param name="handler">The validation rule handler.</param>
    void Register(string key, IValidationRuleHandler handler);

    /// <summary>
    /// Resolves a validation rule handler by key.
    /// </summary>
    /// <param name="key">The unique rule key.</param>
    /// <returns>The resolved validation rule handler.</returns>
    IValidationRuleHandler Resolve(string key);
}
