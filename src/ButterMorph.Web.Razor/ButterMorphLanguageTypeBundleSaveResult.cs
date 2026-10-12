namespace ButterMorph.Web.Razor;

/// <summary>
/// Describes one language type bundle save result.
/// </summary>
public sealed class ButterMorphLanguageTypeBundleSaveResult
{
    /// <summary>
    /// Gets or sets a value indicating whether the bundle was saved.
    /// </summary>
    public bool Succeeded { get; set; } = true;

    /// <summary>
    /// Gets or sets the saved bundle.
    /// </summary>
    public ButterMorphLanguageTypeBundle Bundle { get; set; } = new();

    /// <summary>
    /// Gets or sets a user-facing message.
    /// </summary>
    public string Message { get; set; } = string.Empty;
}
