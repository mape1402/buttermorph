/// <summary>
/// Represents a playground scenario shown by the host shell.
/// </summary>
internal sealed class PlaygroundScenarioSummary
{
    /// <summary>
    /// Gets or sets the scenario context key.
    /// </summary>
    public string ContextKey { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the scenario display name.
    /// </summary>
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the scenario description.
    /// </summary>
    public string Description { get; set; } = string.Empty;
}
