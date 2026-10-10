namespace ButterMorph.StudioPlayground.Models;

/// <summary>
/// Represents validation setup selected by the host before ButterMorph saves.
/// </summary>
internal sealed class StudioValidationSetup
{
    /// <summary>
    /// Gets or sets the host-owned identifier.
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the validation display name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets source schema context keys by alias.
    /// </summary>
    public Dictionary<string, string> SourceSchemaIds { get; set; } = new(StringComparer.OrdinalIgnoreCase);
}
