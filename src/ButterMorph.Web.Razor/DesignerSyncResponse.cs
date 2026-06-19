namespace ButterMorph.Web.Razor;

/// <summary>
/// Represents the designer live synchronization response.
/// </summary>
public sealed class DesignerSyncResponse
{
    /// <summary>
    /// Gets or sets a value indicating whether synchronization succeeded.
    /// </summary>
    public bool Succeeded { get; set; }

    /// <summary>
    /// Gets or sets the user-facing message.
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the current DSL content.
    /// </summary>
    public string DslContent { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets current mapping expressions by target path.
    /// </summary>
    public IReadOnlyDictionary<string, string> Mappings { get; set; } = new Dictionary<string, string>();

    /// <summary>
    /// Gets or sets the current diagnostics count.
    /// </summary>
    public int DiagnosticsCount { get; set; }

    /// <summary>
    /// Gets or sets diagnostics that can be rendered in the DSL editor.
    /// </summary>
    public IReadOnlyCollection<DesignerEditorDiagnostic> EditorDiagnostics { get; set; } = [];

    /// <summary>
    /// Gets or sets a value indicating whether the host save flow completed.
    /// </summary>
    public bool HostSaveCompleted { get; set; }

    /// <summary>
    /// Gets or sets the saved host context key.
    /// </summary>
    public string SavedContextKey { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the safe local return URL for non-popup host flows.
    /// </summary>
    public string SafeReturnUrl { get; set; } = string.Empty;
}
