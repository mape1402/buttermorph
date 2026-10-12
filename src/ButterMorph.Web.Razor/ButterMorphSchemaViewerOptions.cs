namespace ButterMorph.Web.Razor;

/// <summary>
/// Configures the reusable read-only schema viewer.
/// </summary>
public sealed class ButterMorphSchemaViewerOptions
{
    /// <summary>
    /// Gets or sets the enabled schema viewer surfaces.
    /// </summary>
    public ButterMorphSchemaViewerViews EnabledViews { get; set; } = ButterMorphSchemaViewerViews.All;

    /// <summary>
    /// Gets or sets the default schema viewer surface.
    /// </summary>
    public ButterMorphSchemaViewerView DefaultView { get; set; } = ButterMorphSchemaViewerView.UserFriendly;

    /// <summary>
    /// Gets or sets the Monaco loader URL used by the JSON viewer.
    /// </summary>
    public string MonacoLoaderUrl { get; set; } = "https://cdn.jsdelivr.net/npm/monaco-editor@0.45.0/min/vs/loader.js";

    /// <summary>
    /// Gets or sets the Monaco VS base URL used by the JSON viewer.
    /// </summary>
    public string MonacoVsBaseUrl { get; set; } = "https://cdn.jsdelivr.net/npm/monaco-editor@0.45.0/min/vs";
}
