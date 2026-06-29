namespace ButterMorph.StudioPlayground.Models;

/// <summary>
/// Represents a host-owned custom data type.
/// </summary>
internal sealed class StudioCustomType
{
    /// <summary>
    /// Gets or sets the context key used by ButterMorph designers.
    /// </summary>
    public string ContextKey { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the canonical custom type key.
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
    /// Gets or sets the version number.
    /// </summary>
    public string Version { get; set; } = "1.0.0";

    /// <summary>
    /// Gets or sets the base JSON Schema type.
    /// </summary>
    public string BaseType { get; set; } = "string";

    /// <summary>
    /// Gets or sets the save comment.
    /// </summary>
    public string Comment { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the generated JSON Schema.
    /// </summary>
    public string JsonSchema { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the last saved timestamp.
    /// </summary>
    public DateTimeOffset SavedAt { get; set; } = DateTimeOffset.UtcNow;
}
