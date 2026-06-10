namespace ButterMorph.Abstractions;

/// <summary>
/// Represents execution state shared by runtime components.
/// </summary>
public interface IExecutionContext
{
    /// <summary>
    /// Gets the source graphs participating in execution.
    /// </summary>
    IReadOnlyDictionary<string, IStructureGraph> Sources { get; }

    /// <summary>
    /// Gets the diagnostic collector for execution.
    /// </summary>
    IDiagnosticCollector Diagnostics { get; }
}
