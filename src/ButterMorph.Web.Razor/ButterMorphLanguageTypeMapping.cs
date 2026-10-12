namespace ButterMorph.Web.Razor;

/// <summary>
/// Maps one ButterMorph type key to one language-native type.
/// </summary>
public sealed class ButterMorphLanguageTypeMapping
{
    /// <summary>
    /// Gets or sets the ButterMorph type key.
    /// </summary>
    public string ButterMorphType { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the ButterMorph type version.
    /// </summary>
    public string ButterMorphTypeVersion { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the language-native type.
    /// </summary>
    public string LanguageType { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the language-native type version.
    /// </summary>
    public string LanguageTypeVersion { get; set; } = string.Empty;
}
