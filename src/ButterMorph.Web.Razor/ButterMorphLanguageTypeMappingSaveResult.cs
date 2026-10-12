namespace ButterMorph.Web.Razor;

/// <summary>
/// Describes one language type mapping save result.
/// </summary>
public sealed class ButterMorphLanguageTypeMappingSaveResult
{
    /// <summary>
    /// Gets or sets a value indicating whether the document is ready for the host.
    /// </summary>
    public bool Succeeded { get; set; } = true;

    /// <summary>
    /// Gets or sets the prepared mapping document.
    /// </summary>
    public ButterMorphLanguageTypeMappingDocument Document { get; set; } = new();

    /// <summary>
    /// Gets or sets the prepared legacy profile.
    /// </summary>
    public ButterMorphLanguageMappingProfile Profile { get; set; } = new();

    /// <summary>
    /// Gets or sets a user-facing message.
    /// </summary>
    public string Message { get; set; } = string.Empty;
}
