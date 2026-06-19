namespace ButterMorph.Web.Razor;

/// <summary>
/// Represents a schema designer load request.
/// </summary>
public sealed class ButterMorphSchemaDesignerLoadRequest
{
    /// <summary>
    /// Gets or sets the host context key.
    /// </summary>
    public string ContextKey { get; set; } = string.Empty;
}
