namespace ButterMorph.SchemaDesign;

using ButterMorph.Abstractions;

/// <summary>
/// Represents payload schema builder output.
/// </summary>
public sealed class PayloadSchemaDesignResult
{
    /// <summary>
    /// Gets or sets a value indicating whether design succeeded.
    /// </summary>
    public bool Succeeded { get; set; }

    /// <summary>
    /// Gets or sets operation diagnostics.
    /// </summary>
    public IReadOnlyCollection<DiagnosticEntry> Diagnostics { get; set; } = [];

    /// <summary>
    /// Gets or sets the canonical schema key.
    /// </summary>
    public string Key { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the schema name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the schema description.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the payload JSON Schema.
    /// </summary>
    public string JsonSchema { get; set; } = string.Empty;
}
