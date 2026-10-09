namespace ButterMorph.Web.Razor;

/// <summary>
/// Configures the schema viewer surfaces enabled for a host.
/// </summary>
[Flags]
public enum ButterMorphSchemaViewerViews
{
    /// <summary>
    /// No schema viewer surface is enabled.
    /// </summary>
    None = 0,

    /// <summary>
    /// Enables the user-friendly visual schema viewer.
    /// </summary>
    UserFriendly = 1,

    /// <summary>
    /// Enables the raw JSON schema viewer.
    /// </summary>
    Json = 2,

    /// <summary>
    /// Enables both the user-friendly and raw JSON schema viewers.
    /// </summary>
    UserFriendlyAndJson = UserFriendly | Json
}
