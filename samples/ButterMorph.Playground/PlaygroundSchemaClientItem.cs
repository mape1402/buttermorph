/// <summary>
/// Represents schema item state sent from the playground browser storage.
/// </summary>
internal sealed class PlaygroundSchemaClientItem
{
    /// <summary>
    /// Gets or sets the context key.
    /// </summary>
    public string ContextKey { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the item kind.
    /// </summary>
    public string Kind { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the display name.
    /// </summary>
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the item description.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the designer path.
    /// </summary>
    public string DesignerPath { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the JSON Schema content.
    /// </summary>
    public string JsonSchema { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the save timestamp.
    /// </summary>
    public string SavedAt { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the schema type version number.
    /// </summary>
    public string VersionNumber { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the schema type base type.
    /// </summary>
    public string BaseType { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the schema type save comment.
    /// </summary>
    public string Comment { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the metadata key.
    /// </summary>
    public string Key { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the data type.
    /// </summary>
    public string DataType { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the applies-to JSON.
    /// </summary>
    public string AppliesToJson { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the validation JSON.
    /// </summary>
    public string ValidationJson { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a value indicating whether the metadata value is required.
    /// </summary>
    public bool IsRequired { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the metadata field is active.
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// Gets or sets the metadata sort order.
    /// </summary>
    public int SortOrder { get; set; }
}
