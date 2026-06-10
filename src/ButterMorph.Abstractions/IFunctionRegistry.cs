namespace ButterMorph.Abstractions;

/// <summary>
/// Defines registration and lookup behavior for DSL functions.
/// </summary>
public interface IFunctionRegistry
{
    /// <summary>
    /// Registers a function using a unique key.
    /// </summary>
    /// <param name="key">The unique function key.</param>
    /// <param name="function">The function implementation.</param>
    void Register(string key, IFunction function);

    /// <summary>
    /// Resolves a function by key.
    /// </summary>
    /// <param name="key">The unique function key.</param>
    /// <returns>The resolved function.</returns>
    IFunction Resolve(string key);
}
