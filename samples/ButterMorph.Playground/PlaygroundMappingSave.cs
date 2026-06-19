using ButterMorph.Abstractions;

/// <summary>
/// Represents a mapping save captured by the playground host.
/// </summary>
internal sealed class PlaygroundMappingSave
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
    /// Gets or sets the saved transformation document.
    /// </summary>
    public ITransformationDocument Document { get; set; }

    /// <summary>
    /// Gets or sets the save timestamp.
    /// </summary>
    public string SavedAt { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the number of mappings saved.
    /// </summary>
    public int MappingCount { get; set; }
}
