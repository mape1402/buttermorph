namespace ButterMorph.Web.Razor;

/// <summary>
/// Describes one host mapping viewer load request.
/// </summary>
public sealed class ButterMorphMappingViewerLoadRequest
{
    /// <summary>
    /// Gets or sets the host-owned context key.
    /// </summary>
    public string ContextKey { get; set; } = string.Empty;
}
