namespace ButterMorph.Language;

/// <summary>
/// Describes one host-owned language type mapping document.
/// </summary>
public sealed class ButterMorphLanguageTypeMappingDocument
{
    /// <summary>
    /// Gets or sets the stable document key.
    /// </summary>
    public string Key { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the display name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the document description.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the target language key.
    /// </summary>
    public string Language { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets referenced language type bundles.
    /// </summary>
    public List<ButterMorphLanguageTypeMappingBundleReference> Bundles { get; set; } = [];

    /// <summary>
    /// Gets or sets the type mapping rows.
    /// </summary>
    public List<ButterMorphLanguageTypeMappingDocumentEntry> Mappings { get; set; } = [];
}

/// <summary>
/// References one host-owned language type bundle used by a mapping document.
/// </summary>
public sealed class ButterMorphLanguageTypeMappingBundleReference
{
    /// <summary>
    /// Gets or sets the bundle key.
    /// </summary>
    public string Key { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the bundle version when relevant.
    /// </summary>
    public string Version { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the host-specific bundle reference.
    /// </summary>
    public string Reference { get; set; } = string.Empty;
}

/// <summary>
/// Describes one ButterMorph-to-language type mapping entry.
/// </summary>
public sealed class ButterMorphLanguageTypeMappingDocumentEntry
{
    /// <summary>
    /// Gets or sets the ButterMorph source type reference.
    /// </summary>
    public ButterMorphLanguageTypeReference ButterMorph { get; set; } = new();

    /// <summary>
    /// Gets or sets the target language type reference.
    /// </summary>
    public ButterMorphLanguageTypeReference TargetLanguage { get; set; } = new();
}

/// <summary>
/// Describes one type reference inside a language type mapping document.
/// </summary>
public sealed class ButterMorphLanguageTypeReference
{
    /// <summary>
    /// Gets or sets the type key.
    /// </summary>
    public string Key { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the display name.
    /// </summary>
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the type version.
    /// </summary>
    public string Version { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the canonical type reference.
    /// </summary>
    public string Reference { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the type source.
    /// </summary>
    public string Source { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the bundle key for language custom types.
    /// </summary>
    public string BundleKey { get; set; } = string.Empty;
}
