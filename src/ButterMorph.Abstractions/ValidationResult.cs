namespace ButterMorph.Abstractions;

/// <summary>
/// Represents the result of validation.
/// </summary>
public sealed class ValidationResult
{
    /// <summary>
    /// Gets or sets a value indicating whether validation passed.
    /// </summary>
    public bool IsValid { get; set; }

    /// <summary>
    /// Gets or sets diagnostics produced during validation.
    /// </summary>
    public IReadOnlyCollection<DiagnosticEntry> Diagnostics { get; set; } = new List<DiagnosticEntry>();
}
