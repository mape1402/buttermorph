namespace ButterMorph.Web.Razor;

/// <summary>
/// Defines one schema metadata field rendered by the payload schema designer.
/// </summary>
public sealed class SchemaMetadataFieldDefinition
{
    /// <summary>
    /// Gets or sets the metadata key.
    /// </summary>
    public string Key { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the display name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the field description.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the metadata data type.
    /// </summary>
    public SchemaMetadataDataType DataType { get; set; } = SchemaMetadataDataType.String;

    /// <summary>
    /// Gets or sets a value indicating whether the field is required.
    /// </summary>
    public bool IsRequired { get; set; }

    /// <summary>
    /// Gets or sets the default value as JSON or text.
    /// </summary>
    public string DefaultValue { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets allowed scalar values.
    /// </summary>
    public IReadOnlyCollection<string> AllowedValues { get; set; } = [];

    /// <summary>
    /// Gets or sets child fields for structured metadata.
    /// </summary>
    public IReadOnlyCollection<SchemaMetadataFieldDefinition> Children { get; set; } = [];

    /// <summary>
    /// Gets or sets the array item definition.
    /// </summary>
    public SchemaMetadataFieldDefinition ArrayItem { get; set; }
}
