namespace ButterMorph.Web.Razor;

/// <summary>
/// Represents a diagnostic that can be rendered inside the DSL editor.
/// </summary>
public sealed class DesignerEditorDiagnostic
{
    /// <summary>
    /// Gets or sets the diagnostic code.
    /// </summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the diagnostic message.
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the diagnostic severity.
    /// </summary>
    public string Severity { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the associated mapping or validation path.
    /// </summary>
    public string Path { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the one-based editor line.
    /// </summary>
    public int Line { get; set; }

    /// <summary>
    /// Gets or sets the one-based editor column.
    /// </summary>
    public int Column { get; set; }

    /// <summary>
    /// Gets or sets the highlighted text length.
    /// </summary>
    public int Length { get; set; }
}
