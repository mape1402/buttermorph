namespace ButterMorph.Web.Razor;

/// <summary>
/// Defines host integration for the field metadata designer.
/// </summary>
public interface IButterMorphFieldMetadataDesignerHost
{
    /// <summary>
    /// Loads field metadata designer state.
    /// </summary>
    /// <param name="request">The load request.</param>
    /// <returns>The load result.</returns>
    Task<ButterMorphFieldMetadataDesignerLoadResult> Load(ButterMorphFieldMetadataDesignerLoadRequest request);

    /// <summary>
    /// Saves field metadata designer state.
    /// </summary>
    /// <param name="request">The save request.</param>
    /// <returns>The save result.</returns>
    Task<ButterMorphFieldMetadataDesignerSaveResult> Save(ButterMorphFieldMetadataDesignerSaveRequest request);
}
