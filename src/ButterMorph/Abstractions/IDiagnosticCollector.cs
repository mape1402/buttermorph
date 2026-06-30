namespace ButterMorph.Abstractions;

/// <summary>
/// Collects diagnostics produced during execution.
/// </summary>
public interface IDiagnosticCollector
{
    /// <summary>
    /// Gets the collected diagnostics.
    /// </summary>
    IReadOnlyCollection<DiagnosticEntry> Entries { get; }

    /// <summary>
    /// Adds a diagnostic entry.
    /// </summary>
    /// <param name="entry">The diagnostic entry.</param>
    void Add(DiagnosticEntry entry);
}
