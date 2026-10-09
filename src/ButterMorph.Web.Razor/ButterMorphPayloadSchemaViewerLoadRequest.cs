namespace ButterMorph.Web.Razor;

/// <summary>
/// Describes one host schema viewer load request.
/// </summary>
public sealed class ButterMorphPayloadSchemaViewerLoadRequest
{
    /// <summary>
    /// Gets or sets the host-owned context key.
    /// </summary>
    public string ContextKey { get; set; } = string.Empty;
}
