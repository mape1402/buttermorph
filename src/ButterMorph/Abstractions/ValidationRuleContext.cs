namespace ButterMorph.Abstractions;

/// <summary>
/// Represents the context passed to a validation rule handler.
/// </summary>
public sealed class ValidationRuleContext
{
    /// <summary>
    /// Gets or sets the validation rule.
    /// </summary>
    public IValidationRule Rule { get; set; }

    /// <summary>
    /// Gets or sets the resolved structure node.
    /// </summary>
    public IStructureNode Node { get; set; }

    /// <summary>
    /// Gets or sets the resolved rule path.
    /// </summary>
    public string Path { get; set; } = string.Empty;
}
