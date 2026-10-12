namespace ButterMorph.UnitTests;

using ButterMorph.Web.Razor;

/// <summary>
/// Provides a fake language type mapping host for Razor integration tests.
/// </summary>
internal sealed class FakeButterMorphLanguageTypeMappingHost : IButterMorphLanguageTypeMappingHost
{
    internal ButterMorphLanguageTypeMappingLoadResult LoadResult { get; set; } = new();

    internal ButterMorphLanguageTypeMappingSaveResult SaveResult { get; set; } = null;

    internal int LoadCalls { get; private set; }

    internal int SaveCalls { get; private set; }

    internal ButterMorphLanguageMappingProfile SavedProfile { get; private set; } = new();

    public Task<ButterMorphLanguageTypeMappingLoadResult> Load(ButterMorphLanguageTypeMappingLoadRequest request)
    {
        LoadCalls++;
        return Task.FromResult(LoadResult);
    }

    public Task<ButterMorphLanguageTypeMappingSaveResult> SaveProfile(ButterMorphLanguageTypeMappingSaveRequest request)
    {
        SaveCalls++;
        SavedProfile = request.Profile;
        return Task.FromResult(SaveResult ?? new ButterMorphLanguageTypeMappingSaveResult
        {
            Succeeded = true,
            Profile = request.Profile,
            Message = "Saved"
        });
    }
}
