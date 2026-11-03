namespace ButterMorph.Web.Razor;

/// <summary>
/// Represents a request to load a validation designer context from a host application.
/// </summary>
public sealed class ButterMorphValidationDesignerLoadRequest
{
    /// <summary>
    /// Gets or sets the host context key.
    /// </summary>
    public string ContextKey { get; set; } = string.Empty;

    /// <summary>Gets or sets the opaque ButterMorph editor session requesting the snapshot.</summary>
    public string SessionId { get; set; } = string.Empty;
}
