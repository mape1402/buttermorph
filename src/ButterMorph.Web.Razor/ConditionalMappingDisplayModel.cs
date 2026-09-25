namespace ButterMorph.Web.Razor;

/// <summary>
/// Represents a visual mapping editor state for a regular or conditional mapping.
/// </summary>
public sealed class ConditionalMappingDisplayModel
{
    /// <summary>
    /// Gets or sets the editor mode.
    /// </summary>
    public string Mode { get; set; } = "basic";

    /// <summary>
    /// Gets or sets the full expression text.
    /// </summary>
    public string Expression { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the conditional expression.
    /// </summary>
    public string ConditionExpression { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the expression used when the condition is true.
    /// </summary>
    public string ThenExpression { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the expression used when the condition is false.
    /// </summary>
    public string ElseExpression { get; set; } = string.Empty;
}
