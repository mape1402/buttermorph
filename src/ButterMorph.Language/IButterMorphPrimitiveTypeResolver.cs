namespace ButterMorph.Language;

/// <summary>
/// Resolves language primitive types provided by one language adapter.
/// </summary>
public interface IButterMorphPrimitiveTypeResolver
{
    /// <summary>
    /// Gets the target language key.
    /// </summary>
    string LanguageKey { get; }

    /// <summary>
    /// Resolves primitive language types.
    /// </summary>
    /// <returns>The primitive type catalog.</returns>
    IReadOnlyCollection<ButterMorphLanguageType> ResolvePrimitiveTypes();
}
