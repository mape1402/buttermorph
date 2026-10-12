namespace ButterMorph.Web.Razor;

/// <summary>
/// Identifies one schema viewer surface.
/// </summary>
public enum ButterMorphSchemaViewerView
{
    /// <summary>
    /// Shows the user-friendly visual schema viewer.
    /// </summary>
    UserFriendly,

    /// <summary>
    /// Shows the raw JSON schema viewer.
    /// </summary>
    Json,

    /// <summary>
    /// Shows the generated code schema viewer.
    /// </summary>
    Code
}
