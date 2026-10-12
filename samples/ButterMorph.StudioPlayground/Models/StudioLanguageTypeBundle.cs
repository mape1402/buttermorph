namespace ButterMorph.StudioPlayground.Models;

using ButterMorph.Web.Razor;

/// <summary>
/// Represents a host-owned language type bundle.
/// </summary>
internal sealed class StudioLanguageTypeBundle
{
    /// <summary>
    /// Gets or sets the host-owned identifier.
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the bundle language key.
    /// </summary>
    public string LanguageKey { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the bundle metadata.
    /// </summary>
    public ButterMorphLanguageTypeBundle Bundle { get; set; } = new();

    /// <summary>
    /// Gets or sets the last saved timestamp.
    /// </summary>
    public DateTimeOffset SavedAt { get; set; } = DateTimeOffset.UtcNow;
}
