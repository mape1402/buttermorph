namespace ButterMorph.Design;

using ButterMorph.Abstractions;

/// <summary>
/// Represents the result of a design-time mapping operation.
/// </summary>
public sealed class MappingOperationResult : IMappingOperationResult
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
