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
    /// Registers a validation rule handler and descriptor using a unique key.
    /// </summary>
    /// <param name="key">The unique rule key.</param>
    /// <param name="handler">The validation rule handler.</param>
    /// <param name="descriptor">The validation rule descriptor.</param>
    void Register(string key, IValidationRuleHandler handler, IValidationRuleDescriptor descriptor);

    /// <summary>
    /// Resolves a validation rule handler by key.
    /// </summary>
    /// <param name="key">The unique rule key.</param>
    /// <returns>The resolved validation rule handler.</returns>
    IValidationRuleHandler Resolve(string key);

    /// <summary>
    /// Resolves a validation rule descriptor by key.
    /// </summary>
    /// <param name="key">The unique rule key.</param>
    /// <returns>The resolved validation rule descriptor.</returns>
    IValidationRuleDescriptor ResolveDescriptor(string key);

    /// <summary>
    /// Lists registered validation rule descriptors.
    /// </summary>
    /// <returns>The registered validation rule descriptors.</returns>
    IReadOnlyCollection<IValidationRuleDescriptor> ListDescriptors();
}
