namespace ButterMorph.UnitTests;

using ButterMorph.Web.Razor;

/// <summary>
/// Provides a fake validation designer host for Razor integration tests.
/// </summary>
internal sealed class FakeButterMorphValidationDesignerHost : IButterMorphValidationDesignerHost
{
    // Stores load results keyed by context.
    internal Dictionary<string, ButterMorphValidationDesignerLoadResult> LoadResults { get; } = new(StringComparer.Ordinal);

    // Stores the fallback load result.
    internal ButterMorphValidationDesignerLoadResult LoadResult { get; set; } = new();

    // Stores the save result.
    internal ButterMorphValidationDesignerSaveResult SaveResult { get; set; } = new();

    // Counts load calls.
    internal int LoadCalls { get; private set; }

    // Counts save calls.
    internal int SaveCalls { get; private set; }

    // Stores the last save request.
    internal ButterMorphValidationDesignerSaveRequest LastSaveRequest { get; private set; }

    /// <summary>
    /// Loads fake validation designer state.
    /// </summary>
    /// <param name="request">The load request.</param>
    /// <returns>The fake load result.</returns>
    public Task<ButterMorphValidationDesignerLoadResult> Load(ButterMorphValidationDesignerLoadRequest request)
    {
        LoadCalls++;

        if (LoadResults.TryGetValue(request.ContextKey, out ButterMorphValidationDesignerLoadResult result))
        {
            return Task.FromResult(result);
        }

        return Task.FromResult(LoadResult);
    }

    /// <summary>
    /// Saves fake validation designer state.
    /// </summary>
    /// <param name="request">The save request.</param>
    /// <returns>The fake save result.</returns>
    public Task<ButterMorphValidationDesignerSaveResult> Save(ButterMorphValidationDesignerSaveRequest request)
    {
        SaveCalls++;
        LastSaveRequest = request;

        return Task.FromResult(SaveResult);
    }
}
