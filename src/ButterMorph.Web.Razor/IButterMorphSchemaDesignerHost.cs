namespace ButterMorph.Web.Razor;

/// <summary>
/// Provides host integration for the reusable schema designer.
/// </summary>
public interface IButterMorphSchemaDesignerHost
{
    /// <summary>
    /// Loads schema designer state for the requested context.
    /// </summary>
    /// <param name="request">The load request.</param>
    /// <returns>The schema designer load result.</returns>
    Task<ButterMorphSchemaDesignerLoadResult> Load(ButterMorphSchemaDesignerLoadRequest request);

    /// <summary>
    /// Saves schema designer state for the requested context.
    /// </summary>
    /// <param name="request">The save request.</param>
    /// <returns>The schema designer save result.</returns>
    Task<ButterMorphSchemaDesignerSaveResult> Save(ButterMorphSchemaDesignerSaveRequest request);
}
