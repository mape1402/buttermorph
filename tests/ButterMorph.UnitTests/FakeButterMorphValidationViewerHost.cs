namespace ButterMorph.UnitTests;

using ButterMorph.Web.Razor;

/// <summary>
/// Provides a fake validation viewer host for Razor integration tests.
/// </summary>
internal sealed class FakeButterMorphValidationViewerHost : IButterMorphValidationViewerHost
{
    internal ButterMorphValidationViewerLoadResult LoadResult { get; set; } = new();

    internal int LoadCalls { get; private set; }

    public Task<ButterMorphValidationViewerLoadResult> Load(ButterMorphValidationViewerLoadRequest request)
    {
        LoadCalls++;
        return Task.FromResult(LoadResult);
    }
}
