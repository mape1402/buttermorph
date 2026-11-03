namespace ButterMorph.Web.Razor;

/// <summary>
/// Describes how a newly loaded host context must be applied to a local designer workspace.
/// </summary>
public sealed class ButterMorphDesignerSessionRefreshResult
{
    /// <summary>Gets or sets the refresh disposition.</summary>
    public ButterMorphDesignerSessionRefreshDisposition Disposition { get; set; }

    /// <summary>Gets or sets whether the host context changed from the local baseline.</summary>
    public bool HasHostChanges { get; set; }
}

/// <summary>
/// Defines how host content is applied after a designer opens or refreshes.
/// </summary>
public enum ButterMorphDesignerSessionRefreshDisposition
{
    /// <summary>Applies the initial host snapshot.</summary>
    Initial,

    /// <summary>Replaces a clean local workspace with the current host snapshot.</summary>
    Reload,

    /// <summary>Updates schemas only and keeps synchronized local changes.</summary>
    PreserveLocalChanges,

    /// <summary>The host snapshot matches the workspace baseline.</summary>
    Unchanged
}
