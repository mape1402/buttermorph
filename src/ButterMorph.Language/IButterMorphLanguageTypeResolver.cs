namespace ButterMorph.Language;

/// <summary>
/// Resolves all language types available for one language.
/// </summary>
public interface IButterMorphLanguageTypeResolver
{
    /// <summary>
    /// Resolves primitive and host-provided bundle language types.
    /// </summary>
    /// <param name="languageKey">The language key.</param>
    /// <param name="bundles">The host-provided custom type bundles for this context.</param>
    /// <returns>The combined type catalog.</returns>
    Task<IReadOnlyCollection<ButterMorphLanguageType>> ResolveTypes(
        string languageKey,
        IReadOnlyCollection<ButterMorphLanguageTypeBundle> bundles = null);
}
