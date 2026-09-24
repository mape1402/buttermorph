namespace ButterMorph.Design;

/// <summary>
/// Creates validation design sessions.
/// </summary>
public interface IValidationDesignSessionFactory
{
    /// <summary>
    /// Creates a validation design session.
    /// </summary>
    /// <returns>The validation design session.</returns>
    IValidationDesignSession Create();
}
