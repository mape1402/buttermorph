namespace ButterMorph.Web.Razor;

/// <summary>
/// Describes one language type mapping save request.
/// </summary>
public sealed class ButterMorphLanguageTypeMappingSaveRequest
{
    /// <summary>
    /// Gets or sets the document to publish.
    /// </summary>
    public ButterMorphLanguageTypeMappingDocument Document { get; set; } = new();

    /// <summary>
    /// Gets or sets the legacy profile to persist.
    /// </summary>
    public ButterMorphLanguageMappingProfile Profile { get; set; } = new();
}
