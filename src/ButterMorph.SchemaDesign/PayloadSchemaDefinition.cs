namespace ButterMorph.SchemaDesign;

using System.Text.Json;
using System.Text.Json.Serialization;

/// <summary>
/// Represents the clean payload schema definition produced for hosts.
/// </summary>
public sealed class PayloadSchemaDefinition
{
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
    /// Gets or sets the schema version.
    /// </summary>
    public string Version { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the schema version comment.
    /// </summary>
    public string VersionComment { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets open schema metadata.
    /// </summary>
    public IReadOnlyDictionary<string, JsonElement> Metadata { get; set; } = new Dictionary<string, JsonElement>();

    /// <summary>
    /// Gets or sets the root schema type.
    /// </summary>
    public string Type { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the schema properties.
    /// </summary>
    public IReadOnlyDictionary<string, JsonElement> Properties { get; set; } = new Dictionary<string, JsonElement>();

    /// <summary>
    /// Gets or sets reusable schema definitions.
    /// </summary>
    [JsonPropertyName("$defs")]
    public IReadOnlyDictionary<string, JsonElement> Definitions { get; set; } = new Dictionary<string, JsonElement>();
}
