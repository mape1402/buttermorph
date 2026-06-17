namespace ButterMorph.Json.Schema;

using ButterMorph.Abstractions;
using ButterMorph.Core;

/// <summary>
/// Represents the result of a JSON Schema conversion.
/// </summary>
public sealed class JsonSchemaConversionResult
{
    /// <summary>
    /// Gets or sets a value indicating whether conversion succeeded.
    /// </summary>
    public bool Succeeded { get; set; }

    /// <summary>
    /// Gets or sets the converted ButterMorph schema.
    /// </summary>
    public IStructureSchema Schema { get; set; } = new StructureSchema();

    /// <summary>
    /// Gets or sets the converted JSON Schema text.
    /// </summary>
    public string JsonSchema { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets diagnostics produced during conversion.
    /// </summary>
    public IReadOnlyCollection<DiagnosticEntry> Diagnostics { get; set; } = [];
}
