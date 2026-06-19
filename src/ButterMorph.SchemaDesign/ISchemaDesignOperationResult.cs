namespace ButterMorph.SchemaDesign;

using ButterMorph.Abstractions;

/// <summary>
/// Represents the result of a schema design operation.
/// </summary>
public interface ISchemaDesignOperationResult
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
