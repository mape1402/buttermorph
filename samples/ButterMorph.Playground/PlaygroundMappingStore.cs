using System.Collections.Concurrent;

/// <summary>
/// Stores playground mapping saves in memory.
/// </summary>
internal sealed class PlaygroundMappingStore
{
    // Stores latest mapping save by context key.
    private readonly ConcurrentDictionary<string, PlaygroundMappingSave> _saves = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Saves the latest mapping content for a context.
    /// </summary>
    /// <param name="save">The save to store.</param>
    public void Save(PlaygroundMappingSave save)
    {
        _saves[save.ContextKey] = save;
    }

    /// <summary>
    /// Attempts to get the latest save for a context.
    /// </summary>
    /// <param name="contextKey">The context key.</param>
    /// <param name="save">The stored save.</param>
    /// <returns><see langword="true"/> when a save exists.</returns>
    public bool TryGet(string contextKey, out PlaygroundMappingSave save)
    {
        return _saves.TryGetValue(contextKey, out save);
    }
}
