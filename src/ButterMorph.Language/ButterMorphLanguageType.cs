namespace ButterMorph.Language;

/// <summary>
/// Describes one language-native type exposed by a language adapter.
/// </summary>
public sealed class ButterMorphLanguageType
{
    /// <summary>
    /// Gets or sets the language type key.
    /// </summary>
    public string Key { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the display name.
    /// </summary>
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the fully qualified type name.
    /// </summary>
    public string QualifiedName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the language type version.
    /// </summary>
    public string Version { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a short description.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the source category.
    /// </summary>
    public string Source { get; set; } = "primitive";

    /// <summary>
    /// Gets or sets the source bundle key for custom language types.
    /// </summary>
    public string BundleKey { get; set; } = string.Empty;
}
