namespace ButterMorph.Web.Razor;

/// <summary>
/// Represents a request to load a designer context from a host application.
/// </summary>
public sealed class ButterMorphDesignerLoadRequest
{
    /// <summary>
    /// Gets or sets the host context key.
    /// </summary>
    public string ContextKey { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the opaque ButterMorph editor session requesting the snapshot.
    /// The host must treat this as correlation data, not as a persistence key.
    /// </summary>
    public string SessionId { get; set; } = string.Empty;
}
