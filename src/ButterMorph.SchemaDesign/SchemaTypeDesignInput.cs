namespace ButterMorph.SchemaDesign;

/// <summary>
/// Represents editable schema type version input.
/// </summary>
public sealed class SchemaTypeDesignInput
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
    public string VersionNumber { get; set; } = "1.0.0";

    /// <summary>
    /// Gets or sets the base type.
    /// </summary>
    public string BaseType { get; set; } = "string";

    /// <summary>
    /// Gets or sets the minimum text length.
    /// </summary>
    public string MinLength { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the maximum text length.
    /// </summary>
    public string MaxLength { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the regular expression pattern.
    /// </summary>
    public string Pattern { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the minimum numeric value.
    /// </summary>
    public string Minimum { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the maximum numeric value.
    /// </summary>
    public string Maximum { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets numeric precision.
    /// </summary>
    public string Precision { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets numeric scale.
    /// </summary>
    public string Scale { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the minimum array item count.
    /// </summary>
    public string MinItems { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the maximum array item count.
    /// </summary>
    public string MaxItems { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets allowed values as JSON.
    /// </summary>
    public string AllowedValuesJson { get; set; } = "[]";

    /// <summary>
    /// Gets or sets the array item base type.
    /// </summary>
    public string ArrayItemType { get; set; } = "string";

    /// <summary>
    /// Gets or sets the array item custom type version identifier.
    /// </summary>
    public string ArrayItemTypeVersionId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a structured schema JSON for map-shaped types.
    /// </summary>
    public string PayloadSchemaJson { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the save comment.
    /// </summary>
    public string Comment { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets open schema metadata.
    /// </summary>
    public IReadOnlyDictionary<string, string> Metadata { get; set; } = new Dictionary<string, string>();
}
