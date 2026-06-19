/// <summary>
/// Represents mapping content returned to the playground shell.
/// </summary>
internal sealed class PlaygroundMappingView
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
    /// Gets or sets the DSL mapping content.
    /// </summary>
    public string DslContent { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the save timestamp.
    /// </summary>
    public string SavedAt { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the mapping count.
    /// </summary>
    public int MappingCount { get; set; }
}
