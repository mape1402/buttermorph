namespace ButterMorph.Web.Razor;

/// <summary>
/// Represents a schema type designer load request.
/// </summary>
public sealed class ButterMorphSchemaTypeDesignerLoadRequest
{
    /// <summary>
    /// Gets or sets the host context key.
    /// </summary>
    public string ContextKey { get; set; } = string.Empty;
}