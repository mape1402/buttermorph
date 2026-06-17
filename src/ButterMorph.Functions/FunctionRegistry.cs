namespace ButterMorph.Functions;

using ButterMorph.Abstractions;

/// <summary>
/// Stores registered DSL function implementations.
/// </summary>
public sealed class FunctionRegistry : IFunctionRegistry
{
    // Holds function implementations by unique function key.
    private readonly Dictionary<string, IFunction> _functions = new(StringComparer.Ordinal);

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
}
