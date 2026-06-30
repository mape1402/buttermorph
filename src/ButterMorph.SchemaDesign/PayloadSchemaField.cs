namespace ButterMorph.SchemaDesign;

/// <summary>
/// Represents one field used to build a payload schema.
/// </summary>
public sealed class PayloadSchemaField
{
    /// <summary>
    /// Gets or sets the field name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the field data type.
    /// </summary>
    public string DataType { get; set; } = "string";

    /// <summary>
    /// Gets or sets the field description.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a value indicating whether the field is required.
    /// </summary>
    public bool IsRequired { get; set; }

    /// <summary>
    /// Gets or sets the custom type version identifier.
    /// </summary>
    public string CustomTypeVersionId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets field metadata.
    /// </summary>
    public IReadOnlyDictionary<string, string> Metadata { get; set; } = new Dictionary<string, string>();

    /// <summary>
    /// Gets or sets validation keywords.
    /// </summary>
    public IReadOnlyDictionary<string, string> Validation { get; set; } = new Dictionary<string, string>();

    /// <summary>
    /// Gets or sets child fields for object-shaped fields.
    /// </summary>
    public IReadOnlyCollection<PayloadSchemaField> Children { get; set; } = [];

    /// <summary>
    /// Gets or sets the array item field definition.
    /// </summary>
    public PayloadSchemaField ArrayItem { get; set; } = null;
}
