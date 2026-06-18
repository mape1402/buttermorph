namespace ButterMorph.Web.Razor;

/// <summary>
/// Configures the reusable ButterMorph Razor designer.
/// </summary>
public sealed class ButterMorphRazorDesignerOptions
{
    /// <summary>
    /// Gets or sets a value indicating whether schema action buttons are shown by default.
    /// </summary>
    public bool ShowSchemaActions { get; set; } = true;

    /// <summary>
    /// Gets or sets the query parameter used as host context key.
    /// </summary>
    public string ContextQueryParameter { get; set; } = "context";

    /// <summary>
    /// Gets or sets a value indicating whether host preload integration is enabled.
    /// </summary>
    public bool UseHostPreload { get; set; } = true;
}
