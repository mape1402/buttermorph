namespace ButterMorph.UnitTests;

using ButterMorph.Web.Razor;

/// <summary>
/// Provides a fake generic schema designer host for Razor integration tests.
/// </summary>
internal sealed class FakeButterMorphSchemaDesignerHost : IButterMorphSchemaDesignerHost
{
    internal ButterMorphSchemaDesignerLoadResult LoadResult { get; set; } = new();

    internal ButterMorphSchemaDesignerSaveResult SaveResult { get; set; } = new();

    internal int LoadCalls { get; private set; }

    internal int SaveCalls { get; private set; }

    internal ButterMorphSchemaDesignerSaveRequest LastSaveRequest { get; private set; }

    public Task<ButterMorphSchemaDesignerLoadResult> Load(ButterMorphSchemaDesignerLoadRequest request)
    {
        LoadCalls++;
        return Task.FromResult(LoadResult);
    }

    public Task<ButterMorphSchemaDesignerSaveResult> Save(ButterMorphSchemaDesignerSaveRequest request)
    {
        SaveCalls++;
        LastSaveRequest = request;
        return Task.FromResult(SaveResult);
    }
}
