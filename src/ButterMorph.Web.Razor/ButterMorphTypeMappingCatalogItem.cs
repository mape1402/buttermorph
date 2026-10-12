namespace ButterMorph.Web.Razor;

/// <summary>
/// Describes one ButterMorph data type available for language type mapping.
/// </summary>
public sealed class ButterMorphTypeMappingCatalogItem
{
    /// <summary>
    /// Gets or sets the ButterMorph data type key used by generated schemas.
    /// </summary>
    public string Key { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the display name.
    /// </summary>
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the ButterMorph type version.
    /// </summary>
    public string Version { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a short description.
    /// </summary>
    public string Description { get; set; } = string.Empty;
}
