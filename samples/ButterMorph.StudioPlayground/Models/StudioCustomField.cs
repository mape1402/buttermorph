namespace ButterMorph.StudioPlayground.Models;

/// <summary>
/// Represents a host-owned metadata field.
/// </summary>
internal sealed class StudioCustomField
{
    /// <summary>
    /// Gets or sets the host-owned identifier used to correlate ButterMorph designers.
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the metadata key.
    /// </summary>
    public string Key { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the display name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the description.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the metadata data type.
    /// </summary>
    public string DataType { get; set; } = "string";

    /// <summary>
    /// Gets or sets the serialized availability scopes.
    /// </summary>
    public string AppliesToJson { get; set; } = "[]";

    /// <summary>
    /// Gets or sets a value indicating whether the metadata is required.
    /// </summary>
    public bool IsRequired { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the metadata is active.
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Gets or sets the validation JSON.
    /// </summary>
    public string ValidationJson { get; set; } = "{}";

    /// <summary>
    /// Gets or sets nested object field definition JSON.
    /// </summary>
    public string ChildrenDefinitionJson { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the array item data type.
    /// </summary>
    public string ArrayItemDataType { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets array item definition JSON.
    /// </summary>
    public string ArrayItemDefinitionJson { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the serialized ButterMorph result received by the host.
    /// </summary>
    public string ButterMorphResultJson { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the last saved timestamp.
    /// </summary>
    public DateTimeOffset SavedAt { get; set; } = DateTimeOffset.UtcNow;
}

