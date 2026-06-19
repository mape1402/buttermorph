namespace ButterMorph.Web.Razor;

/// <summary>
/// Defines host integration for the schema type designer.
/// </summary>
public interface IButterMorphSchemaTypeDesignerHost
{
    /// <summary>
    /// Loads schema type designer state.
    /// </summary>
    /// <param name="request">The load request.</param>
    /// <returns>The load result.</returns>
    Task<ButterMorphSchemaTypeDesignerLoadResult> Load(ButterMorphSchemaTypeDesignerLoadRequest request);

    /// <summary>
    /// Saves schema type designer state.
    /// </summary>
    /// <param name="request">The save request.</param>
    /// <returns>The save result.</returns>
    Task<ButterMorphSchemaTypeDesignerSaveResult> Save(ButterMorphSchemaTypeDesignerSaveRequest request);
}