namespace ButterMorph.Web.Razor;

using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Options;

/// <summary>
/// Displays a reusable language type mapping designer.
/// </summary>
[IgnoreAntiforgeryToken]
public sealed class LanguageTypeMappingsDesignerModel : PageModel
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        WriteIndented = true
    };

    private readonly ButterMorphRazorDesignerOptions options;
    private readonly IReadOnlyCollection<IButterMorphLanguageAdapter> languageAdapters;
    private readonly IEnumerable<IButterMorphLanguageTypeMappingHost> hosts;
    private readonly IButterMorphLanguageTypeResolver typeResolver;

    /// <summary>
    /// Initializes a new instance of the <see cref="LanguageTypeMappingsDesignerModel"/> class.
    /// </summary>
    /// <param name="options">The reusable designer options.</param>
    /// <param name="languageAdapters">The language adapters.</param>
    /// <param name="hosts">The optional language type mapping hosts.</param>
    /// <param name="typeResolver">The combined language type resolver.</param>
    public LanguageTypeMappingsDesignerModel(
        IOptions<ButterMorphRazorDesignerOptions> options,
        IEnumerable<IButterMorphLanguageAdapter> languageAdapters,
        IEnumerable<IButterMorphLanguageTypeMappingHost> hosts,
        IButterMorphLanguageTypeResolver typeResolver)
    {
        this.options = options.Value;
        this.languageAdapters = languageAdapters
            .OrderBy(adapter => adapter.DisplayName, StringComparer.OrdinalIgnoreCase)
            .ToArray();
        this.hosts = hosts;
        this.typeResolver = typeResolver;
    }

    /// <summary>
    /// Gets the host-configured designer theme style.
    /// </summary>
    public string ThemeStyle => this.BuildThemeStyle(options);

    /// <summary>
    /// Gets the host-configured designer theme mode.
    /// </summary>
    public string ThemeMode => this.ResolveThemeModeName(options);

    /// <summary>
    /// Gets the serialized designer data.
    /// </summary>
    public string MappingDataJson { get; private set; } = "{}";

    /// <summary>
    /// Handles initial display.
    /// </summary>
    /// <returns>The asynchronous page task.</returns>
    public async Task OnGet()
    {
        MappingDataJson = await CreateMappingDataJson();
    }

    /// <summary>
    /// Saves one language type mapping profile.
    /// </summary>
    /// <returns>The save response.</returns>
    public async Task<IActionResult> OnPostSaveProfile()
    {
        ButterMorphLanguageMappingProfile profile = await JsonSerializer.DeserializeAsync<ButterMorphLanguageMappingProfile>(Request.Body, JsonOptions)
            ?? new ButterMorphLanguageMappingProfile();
        foreach (IButterMorphLanguageTypeMappingHost host in hosts)
        {
            ButterMorphLanguageTypeMappingSaveResult result = await host.SaveProfile(new ButterMorphLanguageTypeMappingSaveRequest
            {
                Profile = profile
            });

            return new JsonResult(result, JsonOptions);
        }

        return new JsonResult(new ButterMorphLanguageTypeMappingSaveResult
        {
            Succeeded = true,
            Profile = profile,
            Message = "No language type mapping host is configured. Profile was not persisted."
        }, JsonOptions);
    }

    private async Task<string> CreateMappingDataJson()
    {
        ButterMorphLanguageTypeMappingLoadResult loadResult = await LoadProfiles();
        IReadOnlyCollection<ButterMorphTypeMappingCatalogItem> butterMorphTypes = loadResult.ButterMorphTypes.Count > 0
            ? loadResult.ButterMorphTypes
            : CreateBaseTypeCatalog();
        IReadOnlyCollection<ButterMorphLanguageMappingProfile> profiles = CreateProfiles(loadResult.Profiles, butterMorphTypes);
        ButterMorphLanguageMappingProfile activeProfile = ResolveActiveProfile(profiles, loadResult.ActiveProfileId);
        List<object> languages = [];
        foreach (IButterMorphLanguageAdapter adapter in languageAdapters)
        {
            IReadOnlyCollection<ButterMorphLanguageType> languageTypes = await typeResolver.ResolveTypes(adapter.Key, loadResult.Bundles);
            languages.Add(new
            {
                adapter.Key,
                adapter.DisplayName,
                adapter.FileExtension,
                types = languageTypes
            });
        }

        return JsonSerializer.Serialize(new
        {
            languages,
            butterMorphTypes,
            profiles,
            bundles = loadResult.Bundles,
            activeProfileId = activeProfile?.Id ?? string.Empty
        }, JsonOptions);
    }

    private async Task<ButterMorphLanguageTypeMappingLoadResult> LoadProfiles()
    {
        foreach (IButterMorphLanguageTypeMappingHost host in hosts)
        {
            return await host.Load(new ButterMorphLanguageTypeMappingLoadRequest()) ?? new ButterMorphLanguageTypeMappingLoadResult();
        }

        return new ButterMorphLanguageTypeMappingLoadResult();
    }

    private IReadOnlyCollection<ButterMorphLanguageMappingProfile> CreateProfiles(
        IReadOnlyCollection<ButterMorphLanguageMappingProfile> hostProfiles,
        IReadOnlyCollection<ButterMorphTypeMappingCatalogItem> butterMorphTypes)
    {
        return languageAdapters
            .Select(adapter =>
            {
                ButterMorphLanguageMappingProfile hostProfile = hostProfiles
                    .FirstOrDefault(profile => string.Equals(profile.LanguageKey, adapter.Key, StringComparison.OrdinalIgnoreCase));
                return MergeWithDefaultProfile(CreateDefaultProfile(adapter, butterMorphTypes), hostProfile, butterMorphTypes);
            })
            .ToArray();
    }

    private static ButterMorphLanguageMappingProfile CreateDefaultProfile(
        IButterMorphLanguageAdapter adapter,
        IReadOnlyCollection<ButterMorphTypeMappingCatalogItem> butterMorphTypes)
    {
        ButterMorphLanguageMappingProfile profile = adapter.CreateDefaultProfile(null, "{}");
        profile.Id = adapter.Key + "-type-mappings";
        profile.Key = profile.Id;
        profile.Name = adapter.DisplayName + " Type Mappings";
        profile.Description = "Maps ButterMorph types to " + adapter.DisplayName + " types.";
        profile.LanguageKey = adapter.Key;
        profile.Language = adapter.Key;
        profile.TypeMappings = profile.TypeMappings
            .OrderBy(mapping => mapping.ButterMorphType, StringComparer.OrdinalIgnoreCase)
            .ThenBy(mapping => mapping.ButterMorphTypeVersion, StringComparer.OrdinalIgnoreCase)
            .ToList();
        profile.MemberMappings = [];
        return profile;
    }

    private static ButterMorphLanguageMappingProfile MergeWithDefaultProfile(
        ButterMorphLanguageMappingProfile defaultProfile,
        ButterMorphLanguageMappingProfile hostProfile,
        IReadOnlyCollection<ButterMorphTypeMappingCatalogItem> butterMorphTypes)
    {
        if (hostProfile == null)
        {
            return defaultProfile;
        }

        Dictionary<string, ButterMorphLanguageTypeMapping> mappings = defaultProfile.TypeMappings
            .Where(mapping => !string.IsNullOrWhiteSpace(mapping.ButterMorphType))
            .ToDictionary(mapping => CreateMappingKey(mapping.ButterMorphType, mapping.ButterMorphTypeVersion), StringComparer.OrdinalIgnoreCase);
        foreach (ButterMorphLanguageTypeMapping mapping in hostProfile.TypeMappings)
        {
            if (!string.IsNullOrWhiteSpace(mapping.ButterMorphType))
            {
                mappings[CreateMappingKey(mapping.ButterMorphType, mapping.ButterMorphTypeVersion)] = mapping;
            }
        }

        hostProfile.TypeMappings = mappings.Values
            .OrderBy(mapping => mapping.ButterMorphType, StringComparer.OrdinalIgnoreCase)
            .ThenBy(mapping => mapping.ButterMorphTypeVersion, StringComparer.OrdinalIgnoreCase)
            .ToList();
        hostProfile.Id = string.IsNullOrWhiteSpace(hostProfile.Id) ? defaultProfile.Id : hostProfile.Id;
        hostProfile.Key = string.IsNullOrWhiteSpace(hostProfile.Key) ? defaultProfile.Key : hostProfile.Key;
        hostProfile.Name = string.IsNullOrWhiteSpace(hostProfile.Name) ? defaultProfile.Name : hostProfile.Name;
        hostProfile.Description = string.IsNullOrWhiteSpace(hostProfile.Description) ? defaultProfile.Description : hostProfile.Description;
        hostProfile.LanguageKey = string.IsNullOrWhiteSpace(hostProfile.LanguageKey) ? defaultProfile.LanguageKey : hostProfile.LanguageKey;
        hostProfile.Language = string.IsNullOrWhiteSpace(hostProfile.Language) ? defaultProfile.Language : hostProfile.Language;
        hostProfile.CollectionType = string.IsNullOrWhiteSpace(hostProfile.CollectionType) ? defaultProfile.CollectionType : hostProfile.CollectionType;
        hostProfile.MemberMappings = [];
        return hostProfile;
    }

    private static ButterMorphLanguageMappingProfile ResolveActiveProfile(IReadOnlyCollection<ButterMorphLanguageMappingProfile> profiles, string activeProfileId)
    {
        return profiles.FirstOrDefault(profile => string.Equals(profile.Id, activeProfileId, StringComparison.OrdinalIgnoreCase))
            ?? profiles.FirstOrDefault();
    }

    private static IReadOnlyCollection<ButterMorphTypeMappingCatalogItem> CreateBaseTypeCatalog()
    {
        return
        [
            Type("string", "String", "builtin", "Base text value."),
            Type("number", "Number", "builtin", "Base numeric value."),
            Type("boolean", "Boolean", "builtin", "Base boolean value."),
            Type("date", "Date", "builtin", "Base date value."),
            Type("datetime", "DateTime", "builtin", "Base date-time value."),
            Type("time", "Time", "builtin", "Base time value."),
            Type("timespan", "TimeSpan", "builtin", "Base duration value."),
            Type("object", "Object", "builtin", "Base object value.")
        ];
    }

    private static ButterMorphTypeMappingCatalogItem Type(string key, string displayName, string version, string description)
    {
        return new ButterMorphTypeMappingCatalogItem
        {
            Key = key,
            DisplayName = displayName,
            Version = version,
            Description = description
        };
    }

    private static string CreateMappingKey(string type, string version)
    {
        return (type ?? string.Empty) + "@" + (version ?? string.Empty);
    }
}
