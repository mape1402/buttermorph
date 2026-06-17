namespace ButterMorph.Design;

using ButterMorph.Abstractions;

/// <summary>
/// Exposes registered design-time capabilities.
/// </summary>
public sealed class CapabilityExplorer : ICapabilityExplorer
{
    // Provides function descriptor discovery.
    private readonly IFunctionRegistry _functionRegistry;

    // Provides validation rule descriptor discovery.
    private readonly IValidationRuleRegistry _validationRuleRegistry;

    /// <summary>
    /// Initializes a new instance of the <see cref="CapabilityExplorer"/> class.
    /// </summary>
    /// <param name="functionRegistry">The function registry.</param>
    /// <param name="validationRuleRegistry">The validation rule registry.</param>
    public CapabilityExplorer(IFunctionRegistry functionRegistry, IValidationRuleRegistry validationRuleRegistry)
    {
        _functionRegistry = functionRegistry;
        _validationRuleRegistry = validationRuleRegistry;
    }

    /// <summary>
    /// Lists registered function descriptors.
    /// </summary>
    /// <returns>The function descriptors.</returns>
    public IReadOnlyCollection<IFunctionDescriptor> ListFunctions()
    {
        return _functionRegistry.ListDescriptors();
    }

    /// <summary>
    /// Lists registered validation rule descriptors.
    /// </summary>
    /// <returns>The validation rule descriptors.</returns>
    public IReadOnlyCollection<IValidationRuleDescriptor> ListValidationRules()
    {
        return _validationRuleRegistry.ListDescriptors();
    }
}
