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
    /// Registers a function and descriptor using a unique key.
    /// </summary>
    /// <param name="key">The unique function key.</param>
    /// <param name="function">The function implementation.</param>
    /// <param name="descriptor">The function descriptor.</param>
    void Register(string key, IFunction function, IFunctionDescriptor descriptor);

    /// <summary>
    /// Resolves a function by key.
    /// </summary>
    /// <param name="key">The unique function key.</param>
    /// <returns>The resolved function.</returns>
    IFunction Resolve(string key);

    /// <summary>
    /// Resolves a function descriptor by key.
    /// </summary>
    /// <param name="key">The unique function key.</param>
    /// <returns>The resolved function descriptor.</returns>
    IFunctionDescriptor ResolveDescriptor(string key);

    /// <summary>
    /// Lists registered function descriptors.
    /// </summary>
    /// <returns>The registered function descriptors.</returns>
    IReadOnlyCollection<IFunctionDescriptor> ListDescriptors();
}
