namespace ButterMorph.Web.Razor;

/// <summary>
/// Defines host integration for the schema designer.
/// </summary>
public interface IButterMorphPayloadSchemaDesignerHost
{
    /// <summary>
    /// Loads schema designer state.
    /// </summary>
    /// <param name="request">The load request.</param>
    /// <returns>The load result.</returns>
    Task<ButterMorphPayloadSchemaDesignerLoadResult> Load(ButterMorphPayloadSchemaDesignerLoadRequest request);

    /// <summary>
    /// Saves schema designer state.
    /// </summary>
    /// <param name="request">The save request.</param>
    /// <returns>The save result.</returns>
    Task<ButterMorphPayloadSchemaDesignerSaveResult> Save(ButterMorphPayloadSchemaDesignerSaveRequest request);
}
