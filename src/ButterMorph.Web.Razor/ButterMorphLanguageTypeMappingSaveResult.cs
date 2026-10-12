namespace ButterMorph.Web.Razor;

/// <summary>
/// Describes one language type mapping save result.
/// </summary>
public sealed class ButterMorphLanguageTypeMappingSaveResult
{
    /// <summary>
    /// Gets or sets a value indicating whether the profile was saved.
    /// </summary>
    public bool Succeeded { get; set; } = true;

    /// <summary>
    /// Gets or sets the saved profile.
    /// </summary>
    public ButterMorphLanguageMappingProfile Profile { get; set; } = new();

    /// <summary>
    /// Gets or sets a user-facing message.
    /// </summary>
    public string Message { get; set; } = string.Empty;
}
