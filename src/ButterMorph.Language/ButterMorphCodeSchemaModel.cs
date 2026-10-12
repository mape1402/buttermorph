namespace ButterMorph.Language;

/// <summary>
/// Describes a payload schema in a language-adapter-friendly shape.
/// </summary>
public sealed class ButterMorphCodeSchemaModel
{
    /// <summary>
    /// Gets or sets the schema key.
    /// </summary>
    public string Key { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the schema display name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the schema description.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the root object node.
    /// </summary>
    public ButterMorphCodeSchemaNode Root { get; set; } = new();
}

/// <summary>
/// Describes one schema node for language code generation.
/// </summary>
public sealed class ButterMorphCodeSchemaNode
{
    /// <summary>
    /// Gets or sets the property key.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the dot path from the root schema.
    /// </summary>
    public string Path { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the display data type.
    /// </summary>
    public string DataType { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the schema node kind.
    /// </summary>
    public string Kind { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the schema description.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the referenced data type when this node uses a custom type.
    /// </summary>
    public string Reference { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the referenced item data type when this node is an array.
    /// </summary>
    public string ItemReference { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a value indicating whether this node is required.
    /// </summary>
    public bool IsRequired { get; set; }

    /// <summary>
    /// Gets or sets child properties for object and array-of-object nodes.
    /// </summary>
    public IReadOnlyCollection<ButterMorphCodeSchemaNode> Children { get; set; } = [];
}
