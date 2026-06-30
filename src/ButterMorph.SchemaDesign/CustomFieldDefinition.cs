namespace ButterMorph.SchemaDesign;

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
    /// Gets or sets the metadata data type.
    /// </summary>
    public string DataType { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets serialized target scopes.
    /// </summary>
    public string AppliesToJson { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a value indicating whether the metadata value is required.
    /// </summary>
    public bool IsRequired { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the metadata field is active.
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// Gets or sets validation JSON.
    /// </summary>
    public string ValidationJson { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets object children as JSON Schema text.
    /// </summary>
    public string ChildrenDefinitionJson { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the array item data type.
    /// </summary>
    public string ArrayItemDataType { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets array item definition as JSON Schema text.
    /// </summary>
    public string ArrayItemDefinitionJson { get; set; } = string.Empty;
}
