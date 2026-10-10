namespace ButterMorph.Web.Razor;

/// <summary>
/// Provides mapping document content to the reusable read-only mapping viewer.
/// </summary>
public interface IButterMorphMappingViewerHost
{
    /// <summary>
    /// Loads the mapping viewer state.
    /// </summary>
    /// <param name="request">The mapping viewer load request.</param>
    /// <returns>The mapping viewer load result.</returns>
    Task<ButterMorphMappingViewerLoadResult> Load(ButterMorphMappingViewerLoadRequest request);
}
