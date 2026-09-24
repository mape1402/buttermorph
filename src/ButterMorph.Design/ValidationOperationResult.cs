namespace ButterMorph.Design;

using ButterMorph.Abstractions;

/// <summary>
/// Represents the result of a validation design operation.
/// </summary>
public sealed class ValidationOperationResult : IValidationOperationResult
{
    /// <summary>
    /// Gets or sets a value indicating whether the operation succeeded.
    /// </summary>
    public bool Succeeded { get; set; }

    /// <summary>
    /// Gets or sets operation diagnostics.
    /// </summary>
    public IReadOnlyCollection<DiagnosticEntry> Diagnostics { get; set; } = [];
}
