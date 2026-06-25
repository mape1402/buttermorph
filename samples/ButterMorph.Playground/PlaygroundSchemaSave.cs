using ButterMorph.Abstractions;

/// <summary>
/// Represents a saved playground schema tool result.
/// </summary>
internal sealed class PlaygroundSchemaSave
{
    /// <summary>
    /// Gets or sets the context key.
    /// </summary>
    public string ContextKey { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the schema item kind.
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
    /// Gets or sets the saved schema when available.
    /// </summary>
    public IStructureSchema Schema { get; set; }

    /// <summary>
    /// Gets or sets the generated JSON payload.
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
    /// Gets or sets the schema version comment.
    /// </summary>
    public string VersionComment { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets schema-level metadata JSON.
    /// </summary>
    public string MetadataJson { get; set; } = "{}";

    /// <summary>
    /// Gets or sets the schema or metadata key.
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
