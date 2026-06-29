namespace ButterMorph.StudioPlayground.Services;

/// <summary>
/// Represents schema designer injection configuration.
/// </summary>
internal sealed class StudioInjectionRequest
{
    /// <summary>
    /// Gets or sets injected custom type context keys.
    /// </summary>
    public List<string> CustomTypeKeys { get; set; } = [];

    /// <summary>
    /// Gets or sets injected custom field context keys.
    /// </summary>
    public List<string> CustomFieldKeys { get; set; } = [];
}
