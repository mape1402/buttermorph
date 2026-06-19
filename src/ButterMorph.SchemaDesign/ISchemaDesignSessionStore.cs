namespace ButterMorph.SchemaDesign;

/// <summary>
/// Stores schema design sessions by key.
/// </summary>
public interface ISchemaDesignSessionStore
{
    /// <summary>
    /// Gets an existing session or creates one.
    /// </summary>
    /// <param name="key">The session key.</param>
    /// <returns>The schema design session.</returns>
    ISchemaDesignSession GetOrCreate(string key);
}
