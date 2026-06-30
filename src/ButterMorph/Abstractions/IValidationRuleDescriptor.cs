namespace ButterMorph.Abstractions;

/// <summary>
/// Describes one validation rule for design-time discovery.
/// </summary>
public interface IValidationRuleDescriptor
{
    /// <summary>
    /// Gets the unique validation rule key.
    /// </summary>
    string Key { get; }

    /// <summary>
    /// Gets the display name.
    /// </summary>
    string DisplayName { get; }

    /// <summary>
    /// Gets the validation rule description.
    /// </summary>
    string Description { get; }

    /// <summary>
    /// Gets the target value kind accepted by the rule.
    /// </summary>
    FunctionValueKind ValueKind { get; }

    /// <summary>
    /// Gets a value indicating whether the rule is required by the catalog.
    /// </summary>
    bool IsRequired { get; }

    /// <summary>
    /// Gets the rule parameters.
    /// </summary>
    IReadOnlyCollection<IValidationRuleParameterDescriptor> Parameters { get; }

    /// <summary>
    /// Gets UI and tooling metadata.
    /// </summary>
    IReadOnlyDictionary<string, string> Metadata { get; }
}
