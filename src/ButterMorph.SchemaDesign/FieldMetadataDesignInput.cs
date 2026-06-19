namespace ButterMorph.SchemaDesign;

/// <summary>
/// Represents editable field metadata input.
/// </summary>
public sealed class FieldMetadataDesignInput
{
    /// <summary>
    /// Gets or sets the metadata display name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the metadata key.
    /// </summary>
    public string Key { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the metadata description.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the metadata data type.
    /// </summary>
    public string DataType { get; set; } = "string";

    /// <summary>
    /// Gets or sets the target scopes as newline-delimited text.
    /// </summary>
    public string AppliesTo { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a value indicating whether the metadata value is required.
    /// </summary>
    public bool IsRequired { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the metadata field is active.
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Gets or sets the sort order.
    /// </summary>
    public int SortOrder { get; set; }

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
    /// Gets or sets the minimum date value.
    /// </summary>
    public string DateMinimum { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the maximum date value.
    /// </summary>
    public string DateMaximum { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets allowed values as newline-delimited text.
    /// </summary>
    public string AllowedValues { get; set; } = string.Empty;
}
