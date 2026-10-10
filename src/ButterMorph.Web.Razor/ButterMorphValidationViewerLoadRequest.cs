namespace ButterMorph.Web.Razor;

/// <summary>
/// Describes one host validation viewer load request.
/// </summary>
public sealed class ButterMorphValidationViewerLoadRequest
{
    /// <summary>
    /// Gets or sets the host-owned context key.
    /// </summary>
    public string ContextKey { get; set; } = string.Empty;
}
