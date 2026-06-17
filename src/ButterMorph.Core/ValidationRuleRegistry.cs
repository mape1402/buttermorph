namespace ButterMorph.Core;

using ButterMorph.Abstractions;

/// <summary>
/// Stores registered validation rule handlers.
/// </summary>
public sealed class ValidationRuleRegistry : IValidationRuleRegistry
{
    // Holds validation rule handlers by unique rule key.
    private readonly Dictionary<string, IValidationRuleHandler> _handlers = new(StringComparer.Ordinal);

    // Holds validation rule descriptors by unique rule key.
    private readonly Dictionary<string, IValidationRuleDescriptor> _descriptors = new(StringComparer.Ordinal);

    /// <summary>
    /// Registers a validation rule handler using a unique key.
    /// </summary>
    /// <param name="key">The unique rule key.</param>
    /// <param name="handler">The validation rule handler.</param>
    public void Register(string key, IValidationRuleHandler handler)
    {
        _handlers[key] = handler;
    }

    /// <summary>
    /// Registers a validation rule handler and descriptor using a unique key.
    /// </summary>
    /// <param name="key">The unique rule key.</param>
    /// <param name="handler">The validation rule handler.</param>
    /// <param name="descriptor">The validation rule descriptor.</param>
    public void Register(string key, IValidationRuleHandler handler, IValidationRuleDescriptor descriptor)
    {
        _handlers[key] = handler;
        _descriptors[key] = descriptor;
    }

    /// <summary>
    /// Resolves a validation rule handler by key.
    /// </summary>
    /// <param name="key">The unique rule key.</param>
    /// <returns>The resolved validation rule handler.</returns>
    public IValidationRuleHandler Resolve(string key)
    {
        if (_handlers.TryGetValue(key, out IValidationRuleHandler handler))
        {
            return handler;
        }

        throw new KeyNotFoundException($"Validation rule handler '{key}' was not registered.");
    }

    /// <summary>
    /// Resolves a validation rule descriptor by key.
    /// </summary>
    /// <param name="key">The unique rule key.</param>
    /// <returns>The resolved validation rule descriptor.</returns>
    public IValidationRuleDescriptor ResolveDescriptor(string key)
    {
        if (_descriptors.TryGetValue(key, out IValidationRuleDescriptor descriptor))
        {
            return descriptor;
        }

        throw new KeyNotFoundException($"Validation rule descriptor '{key}' was not registered.");
    }

    /// <summary>
    /// Lists registered validation rule descriptors.
    /// </summary>
    /// <returns>The registered validation rule descriptors.</returns>
    public IReadOnlyCollection<IValidationRuleDescriptor> ListDescriptors()
    {
        return [.. _descriptors.Values];
    }
}
