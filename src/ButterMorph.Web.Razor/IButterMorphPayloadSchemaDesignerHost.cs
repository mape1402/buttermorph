namespace ButterMorph.Web.Razor;

/// <summary>
/// Defines host integration for the payload schema designer.
/// </summary>
public interface IButterMorphPayloadSchemaDesignerHost
{
    /// <summary>
    /// Loads payload schema designer state.
    /// </summary>
    /// <param name="request">The load request.</param>
    /// <returns>The load result.</returns>
    Task<ButterMorphPayloadSchemaDesignerLoadResult> Load(ButterMorphPayloadSchemaDesignerLoadRequest request);

    /// <summary>
    /// Saves payload schema designer state.
    /// </summary>
    /// <param name="request">The save request.</param>
    /// <returns>The save result.</returns>
    Task<ButterMorphPayloadSchemaDesignerSaveResult> Save(ButterMorphPayloadSchemaDesignerSaveRequest request);
}