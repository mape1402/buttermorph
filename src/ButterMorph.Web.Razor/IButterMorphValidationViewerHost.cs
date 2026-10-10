namespace ButterMorph.Web.Razor;

/// <summary>
/// Provides validation document content to the reusable read-only validation viewer.
/// </summary>
public interface IButterMorphValidationViewerHost
{
    /// <summary>
    /// Loads the validation viewer state.
    /// </summary>
    /// <param name="request">The validation viewer load request.</param>
    /// <returns>The validation viewer load result.</returns>
    Task<ButterMorphValidationViewerLoadResult> Load(ButterMorphValidationViewerLoadRequest request);
}
