namespace ButterMorph.Design;

/// <summary>
/// Stores reusable mapping design sessions by key.
/// </summary>
public interface IMappingDesignSessionStore
{
    /// <summary>
    /// Gets an existing session or creates a new one.
    /// </summary>
    /// <param name="key">The session key.</param>
    /// <returns>The mapping design session.</returns>
    IMappingDesignSession GetOrCreate(string key);

    /// <summary>
    /// Removes a session and releases its in-memory state.
    /// </summary>
    /// <param name="key">The session key.</param>
    /// <returns><see langword="true"/> when a session was removed.</returns>
    bool Remove(string key);
}
