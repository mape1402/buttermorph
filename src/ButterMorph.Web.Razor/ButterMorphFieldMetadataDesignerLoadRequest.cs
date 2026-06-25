namespace ButterMorph.Web.Razor;

/// <summary>
/// Represents a field metadata designer load request.
/// </summary>
public sealed class ButterMorphFieldMetadataDesignerLoadRequest
{
    /// <summary>
    /// Gets or sets the host context key.
    /// </summary>
    public string ContextKey { get; set; } = string.Empty;
}
