namespace ButterMorph.Design;

/// <summary>
/// Stores reusable validation design sessions by key.
/// </summary>
public interface IValidationDesignSessionStore
{
    /// <summary>
    /// Gets an existing session or creates a new one.
    /// </summary>
    /// <param name="key">The session key.</param>
    /// <returns>The validation design session.</returns>
    IValidationDesignSession GetOrCreate(string key);
}
