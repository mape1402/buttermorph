namespace ButterMorph.StudioPlayground.Models;

using ButterMorph.Web.Razor;

/// <summary>
/// Represents a host-owned language type mapping profile.
/// </summary>
internal sealed class StudioLanguageTypeProfile
{
    /// <summary>
    /// Gets or sets the host-owned identifier.
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the language key.
    /// </summary>
    public string LanguageKey { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the language type mapping profile.
    /// </summary>
    public ButterMorphLanguageMappingProfile Profile { get; set; } = new();

    /// <summary>
    /// Gets or sets the last saved timestamp.
    /// </summary>
    public DateTimeOffset SavedAt { get; set; } = DateTimeOffset.UtcNow;
}
