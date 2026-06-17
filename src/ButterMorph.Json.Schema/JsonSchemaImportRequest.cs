namespace ButterMorph.Json.Schema;

/// <summary>
/// Represents a JSON Schema import request.
/// </summary>
public sealed class JsonSchemaImportRequest
{
    /// <summary>
    /// Gets or sets the schema name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the JSON Schema text.
    /// </summary>
    public string JsonSchema { get; set; } = string.Empty;
}
