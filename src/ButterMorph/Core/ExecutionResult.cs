using ButterMorph.Abstractions;

namespace ButterMorph.Core;

/// <summary>
/// Represents a generic execution result.
/// </summary>
public sealed class ExecutionResult
{
    /// <summary>
    /// Gets or sets a value indicating whether execution succeeded.
    /// </summary>
    public bool Succeeded { get; set; }

    /// <summary>
    /// Gets or sets diagnostics produced during execution.
    /// </summary>
    public IReadOnlyCollection<DiagnosticEntry> Diagnostics { get; set; } = new List<DiagnosticEntry>();
}
