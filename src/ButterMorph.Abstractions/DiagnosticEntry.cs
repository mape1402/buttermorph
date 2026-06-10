namespace ButterMorph.Abstractions;

/// <summary>
/// Represents a diagnostic produced by ButterMorph.
/// </summary>
public sealed class DiagnosticEntry
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
    /// Gets or sets the structure path associated with the diagnostic.
    /// </summary>
    public string Path { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the diagnostic severity.
    /// </summary>
    public string Severity { get; set; } = string.Empty;
}
