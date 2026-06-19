namespace ButterMorph.SchemaDesign;

/// <summary>
/// Represents one schema type available to designers.
/// </summary>
public sealed class SchemaTypeCatalogItem
{
    /// <summary>
    /// Gets or sets the stable type identifier.
    /// </summary>
    public string TypeId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the stable type version identifier.
    /// </summary>
    public string TypeVersionId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the display name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the version number.
    /// </summary>
    public string VersionNumber { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the JSON Schema base type.
    /// </summary>
    public string BaseType { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the type JSON Schema.
    /// </summary>
    public string JsonSchema { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a value indicating whether this item is a system type.
    /// </summary>
    public bool IsSystem { get; set; }
}