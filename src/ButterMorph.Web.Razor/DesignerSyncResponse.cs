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
}
