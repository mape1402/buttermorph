namespace ButterMorph.Json.Schema;

/// <summary>
/// Represents a JSON Schema import request.
/// </summary>
public sealed class JsonSchemaImportRequest
{
    /// <summary>
    /// Gets or sets the explicit schema key used when the JSON Schema does not contain one.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the explicit schema version used when the JSON Schema does not contain one.
    /// </summary>
    public string Version { get; set; } = "1.0.0";

    /// <summary>
    /// Gets or sets the JSON Schema text.
    /// </summary>
    public string JsonSchema { get; set; } = string.Empty;
}
