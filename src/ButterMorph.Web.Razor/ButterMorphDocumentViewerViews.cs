namespace ButterMorph.Web.Razor;

/// <summary>
/// Configures the DSL-backed document viewer surfaces enabled for a host.
/// </summary>
[Flags]
public enum ButterMorphDocumentViewerViews
{
    /// <summary>
    /// No viewer surface is enabled.
    /// </summary>
    None = 0,

    /// <summary>
    /// Enables the user-friendly visual viewer.
    /// </summary>
    UserFriendly = 1,

    /// <summary>
    /// Enables the raw DSL viewer.
    /// </summary>
    Dsl = 2,

    /// <summary>
    /// Enables both the user-friendly and raw DSL viewers.
    /// </summary>
    UserFriendlyAndDsl = UserFriendly | Dsl
}
