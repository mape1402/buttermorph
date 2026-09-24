/// <summary>
/// Represents validation content returned to the playground shell.
/// </summary>
internal sealed class PlaygroundValidationView
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
    /// Gets or sets the DSL validation content.
    /// </summary>
    public string DslContent { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the save timestamp.
    /// </summary>
    public string SavedAt { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the number of validation statements.
    /// </summary>
    public int ValidationCount { get; set; }
}
