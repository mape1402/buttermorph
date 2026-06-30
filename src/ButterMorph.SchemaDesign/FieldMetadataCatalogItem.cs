namespace ButterMorph.SchemaDesign;

/// <summary>
/// Represents one metadata field available to payload schema designers.
/// </summary>
public sealed class FieldMetadataCatalogItem
{
    /// <summary>
    /// Gets or sets the metadata identifier.
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the display name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the metadata key.
    /// </summary>
    public string Key { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the description.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the metadata field version.
    /// </summary>
    public string Version { get; set; } = "1.0.0";

    /// <summary>
    /// Gets or sets the version comment.
    /// </summary>
    public string VersionComment { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the metadata data type.
    /// </summary>
    public string DataType { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a value indicating whether the metadata field is required.
    /// </summary>
    public bool IsRequired { get; set; }

    /// <summary>
    /// Gets or sets the metadata validation JSON.
    /// </summary>
    public string Validation { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the serialized target scopes.
    /// </summary>
    public string AppliesToJson { get; set; } = string.Empty;

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
