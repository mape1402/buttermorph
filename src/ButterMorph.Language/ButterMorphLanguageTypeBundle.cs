namespace ButterMorph.Language;

/// <summary>
/// Describes a discovered package of language-native custom types.
/// </summary>
public sealed class ButterMorphLanguageTypeBundle
{
    /// <summary>
    /// Gets or sets the generated assembly identifier.
    /// </summary>
    public string AssemblyId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the stable assembly bundle key.
    /// </summary>
    public string AssemblyKey { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the assembly bundle description.
    /// </summary>
    public string AssemblyDescription { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the assembly bundle version.
    /// </summary>
    public string AssemblyVersion { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the target language key.
    /// </summary>
    public string LanguageKey { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets discovered custom language types.
    /// </summary>
    public List<ButterMorphLanguageType> Types { get; set; } = [];
}
