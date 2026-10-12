namespace ButterMorph.Web.Razor;

/// <summary>
/// Provides host-owned language type mappings.
/// </summary>
public interface IButterMorphLanguageTypeMappingHost
{
    /// <summary>
    /// Loads language type mappings.
    /// </summary>
    /// <param name="request">The load request.</param>
    /// <returns>The load result.</returns>
    Task<ButterMorphLanguageTypeMappingLoadResult> Load(ButterMorphLanguageTypeMappingLoadRequest request);

    /// <summary>
    /// Saves one language type mapping profile.
    /// </summary>
    /// <param name="request">The save request.</param>
    /// <returns>The save result.</returns>
    Task<ButterMorphLanguageTypeMappingSaveResult> SaveProfile(ButterMorphLanguageTypeMappingSaveRequest request);
}
