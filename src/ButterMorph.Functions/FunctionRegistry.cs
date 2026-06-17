namespace ButterMorph.Functions;

using ButterMorph.Abstractions;

/// <summary>
/// Stores registered DSL function implementations.
/// </summary>
public sealed class FunctionRegistry : IFunctionRegistry
{
    // Holds function implementations by unique function key.
    private readonly Dictionary<string, IFunction> _functions = new(StringComparer.Ordinal);

    // Holds function descriptors by unique function key.
    private readonly Dictionary<string, IFunctionDescriptor> _descriptors = new(StringComparer.Ordinal);

    /// <summary>
    /// Registers a function using a unique key.
    /// </summary>
    /// <param name="key">The unique function key.</param>
    /// <param name="function">The function implementation.</param>
    public void Register(string key, IFunction function)
    {
        _functions[key] = function;
    }

    /// <summary>
    /// Registers a function and descriptor using a unique key.
    /// </summary>
    /// <param name="key">The unique function key.</param>
    /// <param name="function">The function implementation.</param>
    /// <param name="descriptor">The function descriptor.</param>
    public void Register(string key, IFunction function, IFunctionDescriptor descriptor)
    {
        _functions[key] = function;
        _descriptors[key] = descriptor;
    }

    /// <summary>
    /// Resolves a function by key.
    /// </summary>
    /// <param name="key">The unique function key.</param>
    /// <returns>The resolved function.</returns>
    public IFunction Resolve(string key)
    {
        if (_functions.TryGetValue(key, out IFunction function))
        {
            return function;
        }

        throw new KeyNotFoundException($"Function '{key}' was not registered.");
    }

    /// <summary>
    /// Resolves a function descriptor by key.
    /// </summary>
    /// <param name="key">The unique function key.</param>
    /// <returns>The resolved function descriptor.</returns>
    public IFunctionDescriptor ResolveDescriptor(string key)
    {
        if (_descriptors.TryGetValue(key, out IFunctionDescriptor descriptor))
        {
            return descriptor;
        }

        throw new KeyNotFoundException($"Function descriptor '{key}' was not registered.");
    }

    /// <summary>
    /// Lists registered function descriptors.
    /// </summary>
    /// <returns>The registered function descriptors.</returns>
    public IReadOnlyCollection<IFunctionDescriptor> ListDescriptors()
    {
        return [.. _descriptors.Values];
    }
}
