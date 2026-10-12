namespace ButterMorph.Web.Razor;

/// <summary>
/// Describes one language type mapping save request.
/// </summary>
public sealed class ButterMorphLanguageTypeMappingSaveRequest
{
    /// <summary>
    /// Gets or sets the profile to persist.
    /// </summary>
    public ButterMorphLanguageMappingProfile Profile { get; set; } = new();
}
