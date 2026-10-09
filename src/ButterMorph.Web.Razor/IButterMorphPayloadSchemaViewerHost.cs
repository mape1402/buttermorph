namespace ButterMorph.Web.Razor;

/// <summary>
/// Provides payload schema content to the reusable read-only schema viewer.
/// </summary>
public interface IButterMorphPayloadSchemaViewerHost
{
    /// <summary>
    /// Loads the schema viewer state.
    /// </summary>
    /// <param name="request">The schema viewer load request.</param>
    /// <returns>The schema viewer load result.</returns>
    Task<ButterMorphPayloadSchemaViewerLoadResult> Load(ButterMorphPayloadSchemaViewerLoadRequest request);
}
