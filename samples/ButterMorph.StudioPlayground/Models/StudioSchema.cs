namespace ButterMorph.StudioPlayground.Models;

/// <summary>
/// Represents a host-owned schema and its designer injection settings.
/// </summary>
internal sealed class StudioSchema
{
    /// <summary>
    /// Gets or sets the host-owned identifier used to correlate ButterMorph designers.
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the canonical schema key.
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
    /// Gets or sets the version.
    /// </summary>
    public string Version { get; set; } = "1.0.0";

    /// <summary>
    /// Gets or sets the version comment.
    /// </summary>
    public string VersionComment { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the generated JSON Schema.
    /// </summary>
    public string JsonSchema { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the serialized ButterMorph result received by the host.
    /// </summary>
    public string ButterMorphResultJson { get; set; } = string.Empty;

    /// <summary>
    /// Gets the custom type ids available to this schema designer.
    /// </summary>
    public List<string> InjectedCustomTypeKeys { get; } = [];

    /// <summary>
    /// Gets the custom field ids available to this schema designer.
    /// </summary>
    public List<string> InjectedCustomFieldKeys { get; } = [];

    /// <summary>
    /// Gets or sets the last saved timestamp.
    /// </summary>
    public DateTimeOffset SavedAt { get; set; } = DateTimeOffset.UtcNow;
}

