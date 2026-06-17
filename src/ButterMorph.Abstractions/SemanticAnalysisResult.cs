namespace ButterMorph.Abstractions;

/// <summary>
/// Represents semantic analysis diagnostics.
/// </summary>
public sealed class SemanticAnalysisResult
{
    /// <summary>
    /// Gets or sets a value indicating whether semantic analysis succeeded.
    /// </summary>
    public bool Succeeded { get; set; }

    /// <summary>
    /// Gets or sets semantic diagnostics.
    /// </summary>
    public IReadOnlyCollection<DiagnosticEntry> Diagnostics { get; set; } = [];
}
