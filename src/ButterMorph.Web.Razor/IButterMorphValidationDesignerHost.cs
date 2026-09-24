namespace ButterMorph.Web.Razor;

/// <summary>
/// Provides host application integration for the reusable ButterMorph validation designer.
/// </summary>
public interface IButterMorphValidationDesignerHost
{
    /// <summary>
    /// Loads validation designer state for the requested context.
    /// </summary>
    /// <param name="request">The load request.</param>
    /// <returns>The validation designer load result.</returns>
    Task<ButterMorphValidationDesignerLoadResult> Load(ButterMorphValidationDesignerLoadRequest request);

    /// <summary>
    /// Saves validation designer state for the requested context.
    /// </summary>
    /// <param name="request">The save request.</param>
    /// <returns>The validation designer save result.</returns>
    Task<ButterMorphValidationDesignerSaveResult> Save(ButterMorphValidationDesignerSaveRequest request);
}
