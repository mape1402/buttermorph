using ButterMorph.Abstractions;

namespace ButterMorph.Core;

/// <summary>
/// Represents runtime state for a ButterMorph execution.
/// </summary>
public sealed class ExecutionContext : IExecutionContext
{
    /// <summary>
    /// Gets or sets the source graphs participating in execution.
    /// </summary>
    public IReadOnlyDictionary<string, IStructureGraph> Sources { get; set; } = new Dictionary<string, IStructureGraph>();

    /// <summary>
    /// Gets or sets the diagnostic collector for execution.
    /// </summary>
    public IDiagnosticCollector Diagnostics { get; set; } = new DiagnosticCollection();
}
