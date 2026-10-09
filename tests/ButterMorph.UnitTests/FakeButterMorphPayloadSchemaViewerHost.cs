namespace ButterMorph.UnitTests;

using ButterMorph.Web.Razor;

/// <summary>
/// Provides a fake payload schema viewer host for Razor integration tests.
/// </summary>
internal sealed class FakeButterMorphPayloadSchemaViewerHost : IButterMorphPayloadSchemaViewerHost
{
    internal ButterMorphPayloadSchemaViewerLoadResult LoadResult { get; set; } = new();

    internal int LoadCalls { get; private set; }

    public Task<ButterMorphPayloadSchemaViewerLoadResult> Load(ButterMorphPayloadSchemaViewerLoadRequest request)
    {
        LoadCalls++;
        return Task.FromResult(LoadResult);
    }
}
