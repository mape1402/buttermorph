namespace ButterMorph.Design;

using ButterMorph.Abstractions;

/// <summary>
/// Represents the result of a design-time mapping operation.
/// </summary>
public interface IMappingOperationResult
{
    /// <summary>
    /// Gets a value indicating whether the operation succeeded.
    /// </summary>
    bool Succeeded { get; }

    /// <summary>
    /// Gets operation diagnostics.
    /// </summary>
    IReadOnlyCollection<DiagnosticEntry> Diagnostics { get; }
}
