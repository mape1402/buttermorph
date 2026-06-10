namespace ButterMorph.Core;

using ButterMorph.Abstractions;

/// <summary>
/// Stores registered validation rule handlers.
/// </summary>
public sealed class ValidationRuleRegistry : IValidationRuleRegistry
{
    // Holds validation rule handlers by unique rule key.
    private readonly Dictionary<string, IValidationRuleHandler> _handlers = new(StringComparer.Ordinal);

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
}
