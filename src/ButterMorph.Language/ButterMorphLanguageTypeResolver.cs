namespace ButterMorph.Language;

/// <summary>
/// Combines adapter primitive types and host-provided custom type bundles.
/// </summary>
public sealed class ButterMorphLanguageTypeResolver : IButterMorphLanguageTypeResolver
{
    private readonly IEnumerable<IButterMorphPrimitiveTypeResolver> primitiveResolvers;

    /// <summary>
    /// Initializes a new instance of the <see cref="ButterMorphLanguageTypeResolver"/> class.
    /// </summary>
    /// <param name="primitiveResolvers">The primitive type resolvers.</param>
    public ButterMorphLanguageTypeResolver(IEnumerable<IButterMorphPrimitiveTypeResolver> primitiveResolvers)
    {
        this.primitiveResolvers = primitiveResolvers;
    }

    /// <inheritdoc />
    public Task<IReadOnlyCollection<ButterMorphLanguageType>> ResolveTypes(
        string languageKey,
        IReadOnlyCollection<ButterMorphLanguageTypeBundle> bundles = null)
    {
        List<ButterMorphLanguageType> types = [];
        foreach (IButterMorphPrimitiveTypeResolver resolver in primitiveResolvers)
        {
            if (string.Equals(resolver.LanguageKey, languageKey, StringComparison.OrdinalIgnoreCase))
            {
                types.AddRange(resolver.ResolvePrimitiveTypes());
            }
        }

        foreach (ButterMorphLanguageTypeBundle bundle in bundles ?? [])
        {
            if (!string.Equals(bundle.LanguageKey, languageKey, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            types.AddRange(bundle.Types.Select(type => new ButterMorphLanguageType
            {
                Key = type.Key,
                DisplayName = type.DisplayName,
                QualifiedName = type.QualifiedName,
                Version = type.Version,
                Description = type.Description,
                Source = string.IsNullOrWhiteSpace(type.Source) ? "bundle" : type.Source,
                BundleKey = string.IsNullOrWhiteSpace(type.BundleKey) ? bundle.AssemblyKey : type.BundleKey
            }));
        }

        IReadOnlyCollection<ButterMorphLanguageType> resolvedTypes = types
            .Where(type => !string.IsNullOrWhiteSpace(type.QualifiedName) || !string.IsNullOrWhiteSpace(type.Key))
            .GroupBy(type => ResolveTypeIdentity(type), StringComparer.OrdinalIgnoreCase)
            .Select(group => group.First())
            .OrderBy(type => type.DisplayName, StringComparer.OrdinalIgnoreCase)
            .ToArray();

        return Task.FromResult(resolvedTypes);
    }

    private static string ResolveTypeIdentity(ButterMorphLanguageType type)
    {
        return string.Join("|", type.QualifiedName, type.Version, type.Source, type.BundleKey);
    }
}
