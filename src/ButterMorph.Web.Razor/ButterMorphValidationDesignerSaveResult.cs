namespace ButterMorph.Web.Razor;

using ButterMorph.Abstractions;

/// <summary>
/// Represents the result of saving validation designer state into a host application.
/// </summary>
public sealed class ButterMorphValidationDesignerSaveResult
{
    /// <summary>
    /// Gets or sets a value indicating whether the save operation succeeded.
    /// </summary>
    public bool Succeeded { get; set; } = true;

    /// <summary>
    /// Gets or sets the user-facing save message.
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets save diagnostics.
    /// </summary>
    public IReadOnlyCollection<DiagnosticEntry> Diagnostics { get; set; } = [];

    /// <summary>Gets or sets whether the host rejected the save because its revision changed.</summary>
    public bool IsConflict { get; set; }

    /// <summary>Gets or sets the schema revision after a successful save or conflict lookup.</summary>
    public string SchemaRevision { get; set; } = string.Empty;

    /// <summary>Gets or sets the validation design revision after a successful save or conflict lookup.</summary>
    public string DesignRevision { get; set; } = string.Empty;
}
