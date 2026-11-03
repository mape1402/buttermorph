namespace ButterMorph.Web.Razor;

/// <summary>
/// Coordinates isolated designer workspaces and their retained in-memory state.
/// </summary>
public interface IButterMorphDesignerSessionManager
{
    /// <summary>Registers or reactivates a workspace.</summary>
    ButterMorphDesignerSessionSnapshot Acquire(ButterMorphDesignerSessionHandle handle);

    /// <summary>Marks a workspace as active.</summary>
    void Touch(ButterMorphDesignerSessionHandle handle);

    /// <summary>Marks the synchronized workspace as containing local changes.</summary>
    void MarkDirty(ButterMorphDesignerSessionHandle handle);

    /// <summary>Records a successful host save and establishes a new concurrency baseline.</summary>
    void MarkSaved(ButterMorphDesignerSessionHandle handle, string schemaRevision, string designRevision);

    /// <summary>Compares a current host snapshot with the workspace baseline.</summary>
    ButterMorphDesignerSessionRefreshResult ObserveHostSnapshot(ButterMorphDesignerSessionHandle handle, string schemaRevision, string designRevision);

    /// <summary>Marks every workspace for a host context as stale without creating a new context key.</summary>
    int InvalidateContext(string contextKey, ButterMorphDesignerSessionKind? kind = null);

    /// <summary>Closes a workspace and immediately releases all associated state.</summary>
    bool Close(ButterMorphDesignerSessionHandle handle);

    /// <summary>Marks a workspace as no longer open in a browser.</summary>
    void Deactivate(ButterMorphDesignerSessionHandle handle);

    /// <summary>Releases sessions that exceeded the configured retention policy.</summary>
    int SweepExpired(DateTimeOffset? utcNow = null);

    /// <summary>Gets the current lifecycle state for a workspace.</summary>
    ButterMorphDesignerSessionSnapshot GetSnapshot(ButterMorphDesignerSessionHandle handle);
}
