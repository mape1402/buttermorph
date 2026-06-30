namespace ButterMorph.Core;

using ButterMorph.Abstractions;

/// <summary>
/// Describes one validation rule for design-time discovery.
/// </summary>
public sealed class ValidationRuleDescriptor : IValidationRuleDescriptor
{
    /// <summary>
    /// Gets or sets the unique validation rule key.
    /// </summary>
    public string Key { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the display name.
    /// </summary>
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the validation rule description.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the target value kind accepted by the rule.
    /// </summary>
    public FunctionValueKind ValueKind { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the rule is required by the catalog.
    /// </summary>
    public bool IsRequired { get; set; }

    /// <summary>
    /// Gets or sets the rule parameters.
    /// </summary>
    public IReadOnlyCollection<IValidationRuleParameterDescriptor> Parameters { get; set; } = [];

    /// <summary>
    /// Gets or sets UI and tooling metadata.
    /// </summary>
    public IReadOnlyDictionary<string, string> Metadata { get; set; } = new Dictionary<string, string>();
}
