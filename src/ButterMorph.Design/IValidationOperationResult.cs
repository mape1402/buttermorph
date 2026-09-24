namespace ButterMorph.Design;

using ButterMorph.Abstractions;

/// <summary>
/// Represents the result of a validation design operation.
/// </summary>
public interface IValidationOperationResult
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
