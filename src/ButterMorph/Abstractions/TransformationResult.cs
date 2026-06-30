namespace ButterMorph.Abstractions;

/// <summary>
/// Represents the result of a transformation.
/// </summary>
public sealed class TransformationResult
{
    /// <summary>
    /// Gets or sets a value indicating whether transformation succeeded.
    /// </summary>
    public bool Succeeded { get; set; }

    /// <summary>
    /// Gets or sets the result graph produced by transformation.
    /// </summary>
    public IStructureGraph ResultGraph { get; set; }

    /// <summary>
    /// Gets or sets diagnostics produced during transformation.
    /// </summary>
    public IReadOnlyCollection<DiagnosticEntry> Diagnostics { get; set; } = new List<DiagnosticEntry>();
}
