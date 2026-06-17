namespace ButterMorph.Design;

using ButterMorph.Abstractions;

/// <summary>
/// Exposes registered design-time capabilities.
/// </summary>
public interface ICapabilityExplorer
{
    /// <summary>
    /// Lists registered function descriptors.
    /// </summary>
    /// <returns>The function descriptors.</returns>
    IReadOnlyCollection<IFunctionDescriptor> ListFunctions();

    /// <summary>
    /// Lists registered validation rule descriptors.
    /// </summary>
    /// <returns>The validation rule descriptors.</returns>
    IReadOnlyCollection<IValidationRuleDescriptor> ListValidationRules();
}
