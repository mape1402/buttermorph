namespace ButterMorph.Web.Razor;

using ButterMorph.Abstractions;

/// <summary>
/// Represents a field metadata designer save result.
/// </summary>
public sealed class ButterMorphFieldMetadataDesignerSaveResult
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
