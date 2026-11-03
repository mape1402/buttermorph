namespace ButterMorph.Web.Razor;

/// <summary>
/// Uniquely identifies one in-memory designer workspace.
/// </summary>
public sealed class ButterMorphDesignerSessionHandle
{
    /// <summary>Gets or sets the type of designer.</summary>
    public ButterMorphDesignerSessionKind Kind { get; set; }

    /// <summary>Gets or sets the stable host-owned context key.</summary>
    public string ContextKey { get; set; } = string.Empty;

    /// <summary>Gets or sets the opaque ButterMorph editor session identifier.</summary>
    public string SessionId { get; set; } = string.Empty;

    /// <summary>Gets the internal key used by ButterMorph stores.</summary>
    public string StorageKey => Kind.ToString().ToLowerInvariant() + ":" + SessionId;
}
