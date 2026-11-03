namespace ButterMorph.Web.Razor;

/// <summary>
/// Describes the lifecycle state of one designer workspace.
/// </summary>
public sealed class ButterMorphDesignerSessionSnapshot
{
    /// <summary>Gets or sets the workspace handle.</summary>
    public ButterMorphDesignerSessionHandle Handle { get; set; } = new();

    /// <summary>Gets or sets whether the workspace contains unsaved synchronized changes.</summary>
    public bool IsDirty { get; set; }

    /// <summary>Gets or sets whether the host invalidated this context.</summary>
    public bool IsInvalidated { get; set; }

    /// <summary>Gets or sets whether a remote update is waiting while local changes are preserved.</summary>
    public bool HasPendingRemoteChanges { get; set; }

    /// <summary>Gets or sets the host schema revision used as the optimistic concurrency baseline.</summary>
    public string SchemaRevision { get; set; } = string.Empty;

    /// <summary>Gets or sets the host design revision used as the optimistic concurrency baseline.</summary>
    public string DesignRevision { get; set; } = string.Empty;

    /// <summary>Gets or sets when the workspace was last active.</summary>
    public DateTimeOffset LastActivityUtc { get; set; }
}
