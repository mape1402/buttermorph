namespace ButterMorph.Web.Razor;

/// <summary>
/// Describes the result of discovering language type metadata from an assembly.
/// </summary>
public sealed class ButterMorphLanguageTypeBundleDiscoveryResult
{
    /// <summary>
    /// Gets or sets a value indicating whether discovery succeeded.
    /// </summary>
    public bool Succeeded { get; set; }

    /// <summary>
    /// Gets or sets the discovered bundle.
    /// </summary>
    public ButterMorphLanguageTypeBundle Bundle { get; set; } = new();

    /// <summary>
    /// Gets or sets discovery diagnostics.
    /// </summary>
    public List<string> Diagnostics { get; set; } = [];
}
