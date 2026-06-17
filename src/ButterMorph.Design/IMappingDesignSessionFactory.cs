namespace ButterMorph.Design;

/// <summary>
/// Creates mapping design sessions.
/// </summary>
public interface IMappingDesignSessionFactory
{
    /// <summary>
    /// Creates a mapping design session.
    /// </summary>
    /// <returns>The mapping design session.</returns>
    IMappingDesignSession Create();
}
