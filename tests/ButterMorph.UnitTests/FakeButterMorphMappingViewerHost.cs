namespace ButterMorph.UnitTests;

using ButterMorph.Web.Razor;

/// <summary>
/// Provides a fake mapping viewer host for Razor integration tests.
/// </summary>
internal sealed class FakeButterMorphMappingViewerHost : IButterMorphMappingViewerHost
{
    internal ButterMorphMappingViewerLoadResult LoadResult { get; set; } = new();

    internal int LoadCalls { get; private set; }

    public Task<ButterMorphMappingViewerLoadResult> Load(ButterMorphMappingViewerLoadRequest request)
    {
        LoadCalls++;
        return Task.FromResult(LoadResult);
    }
}
