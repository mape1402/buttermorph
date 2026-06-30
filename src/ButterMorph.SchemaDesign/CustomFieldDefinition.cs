namespace ButterMorph.SchemaDesign;

using System.Text.Json;
using System.Text.Json.Serialization;

/// <summary>
/// Represents the clean custom field definition produced for hosts.
/// </summary>
public sealed class CustomFieldDefinition
{
    /// <summary>
    /// Gets or sets the metadata key.
    /// </summary>
    public string Key { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the metadata display name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the metadata description.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the metadata field version.
    /// </summary>
    public string Version { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the version comment.
    /// </summary>
    public string VersionComment { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the metadata data type.
    /// </summary>
    public string DataType { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets target scopes.
    /// </summary>
    public IReadOnlyCollection<string> AppliesTo { get; set; } = [];

    /// <summary>
    /// Gets or sets a value indicating whether the metadata value is required.
    /// </summary>
    public bool IsRequired { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the metadata field is active.
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// Gets or sets validation data.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public IReadOnlyDictionary<string, JsonElement> Validation { get; set; }

    /// <summary>
    /// Gets or sets object children definition.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public JsonElement ChildrenDefinition { get; set; }

    /// <summary>
    /// Gets or sets the array item data type.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public string ArrayItemDataType { get; set; }

    /// <summary>
    /// Gets or sets array item definition.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public JsonElement ArrayItemDefinition { get; set; }
}
