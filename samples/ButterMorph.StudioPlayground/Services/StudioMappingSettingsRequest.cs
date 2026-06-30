namespace ButterMorph.StudioPlayground.Services;

/// <summary>
/// Represents mapping setup selected by the host.
/// </summary>
internal sealed class StudioMappingSettingsRequest
{
    /// <summary>
    /// Gets or sets the mapping display name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets source schema context keys by alias.
    /// </summary>
    public Dictionary<string, string> SourceSchemaIds { get; set; } = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Gets or sets the target schema context key.
    /// </summary>
    public string TargetSchemaId { get; set; } = string.Empty;
}
