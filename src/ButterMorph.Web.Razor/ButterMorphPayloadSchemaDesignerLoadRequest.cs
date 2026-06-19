namespace ButterMorph.Web.Razor;

/// <summary>
/// Represents a payload schema designer load request.
/// </summary>
public sealed class ButterMorphPayloadSchemaDesignerLoadRequest
{
    /// <summary>
    /// Gets or sets the host context key.
    /// </summary>
    public string ContextKey { get; set; } = string.Empty;
}