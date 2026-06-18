namespace ButterMorph.UnitTests;

using ButterMorph.Web.Razor;

/// <summary>
/// Provides a fake designer host for Razor integration tests.
/// </summary>
internal sealed class FakeButterMorphDesignerHost : IButterMorphDesignerHost
{
    // Stores load results keyed by context.
    internal Dictionary<string, ButterMorphDesignerLoadResult> LoadResults { get; } = new(StringComparer.Ordinal);

    // Stores the fallback load result.
    internal ButterMorphDesignerLoadResult LoadResult { get; set; } = new();

    // Stores the save result.
    internal ButterMorphDesignerSaveResult SaveResult { get; set; } = new();

    // Counts load calls.
    internal int LoadCalls { get; private set; }

    // Counts save calls.
    internal int SaveCalls { get; private set; }

    // Stores the last save request.
    internal ButterMorphDesignerSaveRequest LastSaveRequest { get; private set; }

    /// <summary>
    /// Loads fake designer state.
    /// </summary>
    /// <param name="request">The load request.</param>
    /// <returns>The fake load result.</returns>
    public Task<ButterMorphDesignerLoadResult> Load(ButterMorphDesignerLoadRequest request)
    {
        LoadCalls++;

        if (LoadResults.TryGetValue(request.ContextKey, out ButterMorphDesignerLoadResult result))
        {
            return Task.FromResult(result);
        }

        return Task.FromResult(LoadResult);
    }

    /// <summary>
    /// Saves fake designer state.
    /// </summary>
    /// <param name="request">The save request.</param>
    /// <returns>The fake save result.</returns>
    public Task<ButterMorphDesignerSaveResult> Save(ButterMorphDesignerSaveRequest request)
    {
        SaveCalls++;
        LastSaveRequest = request;

        return Task.FromResult(SaveResult);
    }
}
