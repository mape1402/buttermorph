namespace ButterMorph.Modeling;

/// <summary>
/// Provides validation helpers for modeling builders.
/// </summary>
internal static class Guard
{
    /// <summary>
    /// Ensures a text value is not blank.
    /// </summary>
    /// <param name="value">The text value.</param>
    /// <param name="name">The parameter name.</param>
    internal static void NotBlank(string value, string name)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException($"{name} cannot be blank.", name);
        }
    }

    /// <summary>
    /// Ensures a collection contains at least one item.
    /// </summary>
    /// <param name="value">The collection value.</param>
    /// <param name="name">The parameter name.</param>
    internal static void NotEmpty<T>(IReadOnlyCollection<T> value, string name)
    {
        if (value.Count == 0)
        {
            throw new ArgumentException($"{name} cannot be empty.", name);
        }
    }
}
