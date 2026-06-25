namespace ButterMorph.Web.Razor;

using ButterMorph.Abstractions;

/// <summary>
/// Represents a schema type designer save result.
/// </summary>
public sealed class ButterMorphSchemaTypeDesignerSaveResult
{
    /// <summary>
    /// Gets or sets a value indicating whether save succeeded.
    /// </summary>
    public bool Succeeded { get; set; }

    /// <summary>
    /// Gets or sets a user-facing message.
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets save diagnostics.
    /// </summary>
    public IReadOnlyCollection<DiagnosticEntry> Diagnostics { get; set; } = [];
}
