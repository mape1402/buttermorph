using ButterMorph.Abstractions;

namespace ButterMorph.Core;

/// <summary>
/// Stores diagnostics produced during execution.
/// </summary>
public sealed class DiagnosticCollection : IDiagnosticCollector
{
    // Holds the mutable diagnostic entries while exposing a read-only contract.
    private readonly List<DiagnosticEntry> entries = new();

    /// <summary>
    /// Gets the collected diagnostics.
    /// </summary>
    public IReadOnlyCollection<DiagnosticEntry> Entries => entries;

    /// <summary>
    /// Adds a diagnostic entry.
    /// </summary>
    /// <param name="entry">The diagnostic entry.</param>
    public void Add(DiagnosticEntry entry)
    {
        entries.Add(entry);
    }
}
