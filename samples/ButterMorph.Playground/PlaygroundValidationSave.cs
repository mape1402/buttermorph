using ButterMorph.Abstractions;

/// <summary>
/// Represents a validation document save captured by the playground host.
/// </summary>
internal sealed class PlaygroundValidationSave
{
    /// <summary>
    /// Gets or sets the saved context key.
    /// </summary>
    public string ContextKey { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the saved DSL content.
    /// </summary>
    public string DslContent { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the saved validation document.
    /// </summary>
    public IValidationDocument Document { get; set; }

    /// <summary>
    /// Gets or sets the save timestamp.
    /// </summary>
    public string SavedAt { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the number of validation statements saved.
    /// </summary>
    public int ValidationCount { get; set; }
}
