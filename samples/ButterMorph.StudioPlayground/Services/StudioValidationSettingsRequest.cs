namespace ButterMorph.StudioPlayground.Services;

/// <summary>
/// Represents validation setup selected by the host.
/// </summary>
internal sealed class StudioValidationSettingsRequest
{
    /// <summary>
    /// Gets or sets the validation display name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets source schema context keys by alias.
    /// </summary>
    public Dictionary<string, string> SourceSchemaIds { get; set; } = new(StringComparer.OrdinalIgnoreCase);
}
