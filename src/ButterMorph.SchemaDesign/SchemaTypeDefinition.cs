namespace ButterMorph.SchemaDesign;

using System.Text.Json;
using System.Text.Json.Serialization;

/// <summary>
/// Represents the clean custom type definition produced for hosts.
/// </summary>
public sealed class SchemaTypeDefinition
{
    /// <summary>
    /// Gets or sets the canonical type key.
    /// </summary>
    public string Key { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the type name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the type description.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the version number.
    /// </summary>
    public string Version { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the base type.
    /// </summary>
    public string BaseType { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the save comment.
    /// </summary>
    public string Comment { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the generated JSON Schema as structured JSON.
    /// </summary>
    public JsonElement Schema { get; set; }

    /// <summary>
    /// Gets or sets the generated JSON Schema text used by host adapters.
    /// </summary>
    [JsonIgnore]
    public string JsonSchema { get; set; } = string.Empty;
}
