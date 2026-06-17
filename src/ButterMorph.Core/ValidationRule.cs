namespace ButterMorph.Core;

using ButterMorph.Abstractions;

/// <summary>
/// Represents a validation rule declaration.
/// </summary>
public sealed class ValidationRule : IValidationRule
{
    /// <summary>
    /// Gets or sets the graph path validated by the rule.
    /// </summary>
    public string Path { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the registered rule handler key.
    /// </summary>
    public string RuleKey { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the rule argument expressions.
    /// </summary>
    public IReadOnlyCollection<ITransformationExpression> Arguments { get; set; } = [];
}
