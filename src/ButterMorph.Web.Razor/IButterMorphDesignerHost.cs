namespace ButterMorph.Web.Razor;

/// <summary>
/// Provides host application integration for the reusable ButterMorph designer.
/// </summary>
public interface IButterMorphDesignerHost
{
    /// <summary>
    /// Loads designer state for the requested context.
    /// </summary>
    /// <param name="request">The load request.</param>
    /// <returns>The designer load result.</returns>
    Task<ButterMorphDesignerLoadResult> Load(ButterMorphDesignerLoadRequest request);

    /// <summary>
    /// Saves designer state for the requested context.
    /// </summary>
    /// <param name="request">The save request.</param>
    /// <returns>The designer save result.</returns>
    Task<ButterMorphDesignerSaveResult> Save(ButterMorphDesignerSaveRequest request);
}
