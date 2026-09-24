namespace ButterMorph.Web.Razor;

/// <summary>
/// Represents an editable validation rule row.
/// </summary>
public sealed class ValidationRuleDisplayModel
{
    /// <summary>
    /// Gets or sets the target path.
    /// </summary>
    public string Path { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the validation rule key.
    /// </summary>
    public string RuleKey { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the rule arguments as DSL text.
    /// </summary>
    public string Arguments { get; set; } = string.Empty;
}
