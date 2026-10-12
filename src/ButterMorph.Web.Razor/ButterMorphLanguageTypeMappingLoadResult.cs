namespace ButterMorph.Web.Razor;

/// <summary>
/// Describes host-owned language type mappings.
/// </summary>
public sealed class ButterMorphLanguageTypeMappingLoadResult
{
    /// <summary>
    /// Gets or sets available ButterMorph data types.
    /// </summary>
    public IReadOnlyCollection<ButterMorphTypeMappingCatalogItem> ButterMorphTypes { get; set; } = [];

    /// <summary>
    /// Gets or sets host-owned language mapping profiles.
    /// </summary>
    public IReadOnlyCollection<ButterMorphLanguageMappingProfile> Profiles { get; set; } = [];

    /// <summary>
    /// Gets or sets registered custom language type bundles.
    /// </summary>
    public IReadOnlyCollection<ButterMorphLanguageTypeBundle> Bundles { get; set; } = [];

    /// <summary>
    /// Gets or sets the active profile id.
    /// </summary>
    public string ActiveProfileId { get; set; } = string.Empty;
}
