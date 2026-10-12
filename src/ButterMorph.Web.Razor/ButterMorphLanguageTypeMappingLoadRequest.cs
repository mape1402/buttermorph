namespace ButterMorph.Web.Razor;

/// <summary>
/// Describes one language type mapping load request.
/// </summary>
public sealed class ButterMorphLanguageTypeMappingLoadRequest
{
    /// <summary>
    /// Gets or sets the host-owned document key.
    /// </summary>
    public string ContextKey { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the selected language key.
    /// </summary>
    public string LanguageKey { get; set; } = string.Empty;
}
