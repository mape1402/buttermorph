namespace ButterMorph.Json.Schema;

using ButterMorph.Abstractions;

/// <summary>
/// Represents a JSON Schema export request.
/// </summary>
public sealed class JsonSchemaExportRequest
{
    /// <summary>
    /// Gets or sets the schema to export.
    /// </summary>
    public IStructureSchema Schema { get; set; }
}
