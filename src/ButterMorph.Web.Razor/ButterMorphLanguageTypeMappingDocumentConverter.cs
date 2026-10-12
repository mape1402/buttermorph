namespace ButterMorph.Web.Razor;

/// <summary>
/// Converts between host-owned type mapping documents and adapter code-generation profiles.
/// </summary>
public static class ButterMorphLanguageTypeMappingDocumentConverter
{
    private static readonly HashSet<string> BuiltInButterMorphTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "string",
        "number",
        "boolean",
        "date",
        "datetime",
        "time",
        "timespan",
        "object"
    };

    /// <summary>
    /// Converts a legacy code-generation profile into a host-owned mapping document.
    /// </summary>
    /// <param name="profile">The legacy profile.</param>
    /// <returns>The converted document.</returns>
    public static ButterMorphLanguageTypeMappingDocument FromProfile(ButterMorphLanguageMappingProfile profile)
    {
        if (profile == null)
        {
            return new ButterMorphLanguageTypeMappingDocument();
        }

        string language = ResolveProfileLanguage(profile);
        string key = FirstNonEmpty(profile.Key, profile.Id, language + "-type-mappings");
        return new ButterMorphLanguageTypeMappingDocument
        {
            Key = key,
            Name = FirstNonEmpty(profile.Name, key),
            Description = profile.Description ?? string.Empty,
            Language = language,
            Mappings = (profile.TypeMappings ?? [])
                .Where(mapping => !string.IsNullOrWhiteSpace(mapping.ButterMorphType) && !string.IsNullOrWhiteSpace(mapping.LanguageType))
                .Select(mapping => new ButterMorphLanguageTypeMappingDocumentEntry
                {
                    ButterMorph = new ButterMorphLanguageTypeReference
                    {
                        Key = mapping.ButterMorphType,
                        Version = mapping.ButterMorphTypeVersion ?? string.Empty,
                        Reference = CreateButterMorphReference(mapping.ButterMorphType, mapping.ButterMorphTypeVersion)
                    },
                    TargetLanguage = new ButterMorphLanguageTypeReference
                    {
                        Key = mapping.LanguageType,
                        Version = mapping.LanguageTypeVersion ?? string.Empty,
                        Reference = mapping.LanguageType
                    }
                })
                .ToList()
        };
    }

    /// <summary>
    /// Converts a host-owned mapping document into a code-generation profile.
    /// </summary>
    /// <param name="document">The mapping document.</param>
    /// <param name="defaultProfile">The adapter defaults to merge into the generated profile.</param>
    /// <returns>The converted profile.</returns>
    public static ButterMorphLanguageMappingProfile ToProfile(
        ButterMorphLanguageTypeMappingDocument document,
        ButterMorphLanguageMappingProfile defaultProfile = null)
    {
        ButterMorphLanguageTypeMappingDocument resolvedDocument = document ?? new ButterMorphLanguageTypeMappingDocument();
        ButterMorphLanguageMappingProfile profile = defaultProfile ?? new ButterMorphLanguageMappingProfile();
        profile.Id = FirstNonEmpty(resolvedDocument.Key, profile.Id, profile.Key);
        profile.Key = FirstNonEmpty(resolvedDocument.Key, profile.Key, profile.Id);
        profile.Name = FirstNonEmpty(resolvedDocument.Name, profile.Name, profile.Key);
        profile.Description = FirstNonEmpty(resolvedDocument.Description, profile.Description);
        profile.LanguageKey = FirstNonEmpty(resolvedDocument.Language, profile.LanguageKey, profile.Language);
        profile.Language = FirstNonEmpty(resolvedDocument.Language, profile.Language, profile.LanguageKey);
        profile.TypeMappings = (resolvedDocument.Mappings ?? [])
            .Where(mapping => !string.IsNullOrWhiteSpace(mapping.ButterMorph?.Key) && HasTargetLanguage(mapping.TargetLanguage))
            .Select(mapping => new ButterMorphLanguageTypeMapping
            {
                ButterMorphType = mapping.ButterMorph.Key,
                ButterMorphTypeVersion = mapping.ButterMorph.Version ?? string.Empty,
                LanguageType = ResolveTargetReference(mapping.TargetLanguage),
                LanguageTypeVersion = mapping.TargetLanguage.Version ?? string.Empty
            })
            .OrderBy(mapping => mapping.ButterMorphType, StringComparer.OrdinalIgnoreCase)
            .ThenBy(mapping => mapping.ButterMorphTypeVersion, StringComparer.OrdinalIgnoreCase)
            .ToList();
        profile.MemberMappings ??= [];
        return profile;
    }

    /// <summary>
    /// Normalizes one host-owned mapping document in place.
    /// </summary>
    /// <param name="document">The mapping document.</param>
    public static void Normalize(ButterMorphLanguageTypeMappingDocument document)
    {
        if (document == null)
        {
            return;
        }

        document.Language = document.Language ?? string.Empty;
        document.Key = FirstNonEmpty(document.Key, document.Language + "-type-mappings");
        document.Name = FirstNonEmpty(document.Name, document.Key);
        document.Description ??= string.Empty;
        document.Bundles = (document.Bundles ?? [])
            .Where(bundle => !string.IsNullOrWhiteSpace(bundle.Key))
            .GroupBy(bundle => bundle.Key, StringComparer.OrdinalIgnoreCase)
            .Select(group => group.First())
            .OrderBy(bundle => bundle.Key, StringComparer.OrdinalIgnoreCase)
            .ToList();
        document.Mappings = (document.Mappings ?? [])
            .Where(mapping => !string.IsNullOrWhiteSpace(mapping.ButterMorph?.Key) && HasTargetLanguage(mapping.TargetLanguage))
            .Select(NormalizeEntry)
            .GroupBy(mapping => CreateMappingIdentity(mapping), StringComparer.OrdinalIgnoreCase)
            .Select(group => group.First())
            .OrderBy(mapping => mapping.ButterMorph.Key, StringComparer.OrdinalIgnoreCase)
            .ThenBy(mapping => mapping.ButterMorph.Version, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    private static ButterMorphLanguageTypeMappingDocumentEntry NormalizeEntry(ButterMorphLanguageTypeMappingDocumentEntry entry)
    {
        entry.ButterMorph ??= new ButterMorphLanguageTypeReference();
        entry.TargetLanguage ??= new ButterMorphLanguageTypeReference();
        if (BuiltInButterMorphTypes.Contains(entry.ButterMorph.Key) && string.IsNullOrWhiteSpace(entry.ButterMorph.Version))
        {
            entry.ButterMorph.Version = "builtin";
        }

        entry.ButterMorph.Reference = FirstNonEmpty(
            CleanReference(entry.ButterMorph.Reference),
            CreateButterMorphReference(entry.ButterMorph.Key, entry.ButterMorph.Version));
        entry.TargetLanguage.Reference = FirstNonEmpty(entry.TargetLanguage.Reference, entry.TargetLanguage.Key);
        entry.TargetLanguage.Key = FirstNonEmpty(entry.TargetLanguage.Key, entry.TargetLanguage.Reference);
        entry.ButterMorph.DisplayName ??= string.Empty;
        entry.ButterMorph.Version ??= string.Empty;
        entry.ButterMorph.Source ??= string.Empty;
        entry.ButterMorph.BundleKey ??= string.Empty;
        entry.TargetLanguage.DisplayName ??= string.Empty;
        entry.TargetLanguage.Version ??= string.Empty;
        entry.TargetLanguage.Source ??= string.Empty;
        entry.TargetLanguage.BundleKey ??= string.Empty;
        return entry;
    }

    private static string CreateMappingIdentity(ButterMorphLanguageTypeMappingDocumentEntry entry)
    {
        return string.Join("@@", entry.ButterMorph.Key, entry.ButterMorph.Version).ToLowerInvariant();
    }

    private static string ResolveProfileLanguage(ButterMorphLanguageMappingProfile profile)
    {
        return FirstNonEmpty(profile.LanguageKey, profile.Language);
    }

    private static bool HasTargetLanguage(ButterMorphLanguageTypeReference reference)
    {
        return !string.IsNullOrWhiteSpace(reference?.Reference) || !string.IsNullOrWhiteSpace(reference?.Key);
    }

    private static string ResolveTargetReference(ButterMorphLanguageTypeReference reference)
    {
        return FirstNonEmpty(reference.Reference, reference.Key);
    }

    private static string CreateButterMorphReference(string key, string version)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            return string.Empty;
        }

        return string.IsNullOrWhiteSpace(version)
            ? "#/dataTypes/" + key
            : "#/dataTypes/" + key + "@" + version;
    }

    private static string CleanReference(string reference)
    {
        return string.IsNullOrWhiteSpace(reference) || reference.EndsWith("@", StringComparison.Ordinal)
            ? string.Empty
            : reference;
    }

    private static string FirstNonEmpty(params string[] values)
    {
        foreach (string value in values)
        {
            if (!string.IsNullOrWhiteSpace(value))
            {
                return value;
            }
        }

        return string.Empty;
    }
}
