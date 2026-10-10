namespace ButterMorph.Web.Razor;

/// <summary>
/// Configures a reusable read-only DSL-backed document viewer.
/// </summary>
public sealed class ButterMorphDocumentViewerOptions
{
    /// <summary>
    /// Gets or sets the enabled viewer surfaces.
    /// </summary>
    public ButterMorphDocumentViewerViews EnabledViews { get; set; } = ButterMorphDocumentViewerViews.UserFriendlyAndDsl;

    /// <summary>
    /// Gets or sets the default viewer surface.
    /// </summary>
    public ButterMorphDocumentViewerView DefaultView { get; set; } = ButterMorphDocumentViewerView.UserFriendly;
}
