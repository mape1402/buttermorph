namespace ButterMorph.Web.Razor;

using System.Text.Json;
using ButterMorph.SchemaDesign;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Options;

/// <summary>
/// Displays a reusable read-only payload schema viewer.
/// </summary>
[IgnoreAntiforgeryToken]
public sealed class PayloadSchemaViewerModel : PageModel
{
    private static readonly JsonSerializerOptions PrettyJsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        WriteIndented = true
    };

    private static readonly JsonSerializerOptions ViewerJsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true
    };

    private readonly ButterMorphRazorDesignerOptions options;
    private readonly IEnumerable<IButterMorphPayloadSchemaViewerHost> hosts;
    private readonly IReadOnlyCollection<IButterMorphLanguageAdapter> languageAdapters;
    private readonly IEnumerable<IButterMorphLanguageTypeMappingHost> typeMappingHosts;

    /// <summary>
    /// Initializes a new instance of the <see cref="PayloadSchemaViewerModel"/> class.
    /// </summary>
    /// <param name="options">The reusable designer options.</param>
    /// <param name="hosts">The optional schema viewer hosts.</param>
    /// <param name="languageAdapters">The optional language adapters.</param>
    /// <param name="typeMappingHosts">The optional language type mapping hosts.</param>
    public PayloadSchemaViewerModel(
        IOptions<ButterMorphRazorDesignerOptions> options,
        IEnumerable<IButterMorphPayloadSchemaViewerHost> hosts,
        IEnumerable<IButterMorphLanguageAdapter> languageAdapters,
        IEnumerable<IButterMorphLanguageTypeMappingHost> typeMappingHosts)
    {
        this.options = options.Value;
        this.hosts = hosts;
        this.languageAdapters = languageAdapters
            .OrderBy(adapter => adapter.DisplayName, StringComparer.OrdinalIgnoreCase)
            .ToArray();
        this.typeMappingHosts = typeMappingHosts;
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
    /// Gets the rendered schema display model.
    /// </summary>
    internal SchemaViewerDisplayModel Viewer { get; private set; } = new();

    /// <summary>
    /// Gets the serialized schema display model for client interaction.
    /// </summary>
    public string ViewerDataJson { get; private set; } = "{}";

    /// <summary>
    /// Gets the pretty raw schema JSON.
    /// </summary>
    public string RawJson { get; private set; } = "{}";

    /// <summary>
    /// Gets the serialized code generation model for client interaction.
    /// </summary>
    public string CodeDataJson { get; private set; } = "{}";

    /// <summary>
    /// Gets a user-facing message.
    /// </summary>
    public string Message { get; private set; } = string.Empty;

    /// <summary>
    /// Gets a value indicating whether the user-friendly viewer is enabled.
    /// </summary>
    public bool UserFriendlyEnabled => HasView(ButterMorphSchemaViewerViews.UserFriendly);

    /// <summary>
    /// Gets a value indicating whether the raw JSON viewer is enabled.
    /// </summary>
    public bool JsonEnabled => HasView(ButterMorphSchemaViewerViews.Json);

    /// <summary>
    /// Gets a value indicating whether the code viewer is enabled.
    /// </summary>
    public bool CodeEnabled => HasView(ButterMorphSchemaViewerViews.Code) && languageAdapters.Count > 0;

    /// <summary>
    /// Gets a value indicating whether the view switch should be shown.
    /// </summary>
    public bool ShowViewSwitch => EnabledViewCount > 1;

    /// <summary>
    /// Gets the active viewer surface.
    /// </summary>
    public string ActiveView { get; private set; } = "ux";

    /// <summary>
    /// Handles initial display.
    /// </summary>
    /// <returns>The asynchronous page task.</returns>
    public async Task OnGet()
    {
        ButterMorphPayloadSchemaViewerLoadResult result = await LoadSchema();
        RawJson = CreateRawJson(result);
        Viewer = SchemaViewerDisplayBuilder.Build(result.Definition, RawJson, result.SchemaTypes);
        ViewerDataJson = JsonSerializer.Serialize(Viewer, ViewerJsonOptions);
        CodeDataJson = await CreateCodeDataJson(result.Definition);
        Message = result.Message;
        ActiveView = ResolveActiveView();
    }

    /// <summary>
    /// Generates code from a posted language mapping profile.
    /// </summary>
    /// <returns>The generated code response.</returns>
    public async Task<IActionResult> OnPostGenerateCode()
    {
        ButterMorphLanguageMappingProfile profile = await ReadProfile();
        ButterMorphPayloadSchemaViewerLoadResult schema = await LoadSchema();
        IButterMorphLanguageAdapter adapter = ResolveAdapter(profile.LanguageKey);
        if (adapter == null)
        {
            return new JsonResult(new
            {
                succeeded = false,
                code = string.Empty,
                diagnostics = new[] { "Language adapter was not found." }
            }, ViewerJsonOptions);
        }

        ButterMorphCodeGenerationResult result = adapter.GenerateCode(schema.Definition, CreateRawJson(schema), profile);
        return new JsonResult(new
        {
            result.Succeeded,
            result.Code,
            result.Diagnostics
        }, ViewerJsonOptions);
    }

    private async Task<ButterMorphPayloadSchemaViewerLoadResult> LoadSchema()
    {
        foreach (IButterMorphPayloadSchemaViewerHost host in hosts)
        {
            ButterMorphPayloadSchemaViewerLoadResult result = await host.Load(new ButterMorphPayloadSchemaViewerLoadRequest
            {
                ContextKey = DesignerSessionKeyResolver.ResolveContextKey(this, options)
            });

            return result ?? new ButterMorphPayloadSchemaViewerLoadResult();
        }

        return new ButterMorphPayloadSchemaViewerLoadResult
        {
            Message = "No schema viewer host is configured."
        };
    }

    private async Task<string> CreateCodeDataJson(PayloadSchemaDefinition definition)
    {
        if (!CodeEnabled)
        {
            return "{}";
        }

        ButterMorphLanguageTypeMappingLoadResult loadResult = await LoadTypeMappings();
        string activeMappingKey = string.IsNullOrWhiteSpace(loadResult.ActiveDocumentKey)
            ? loadResult.ActiveProfileId
            : loadResult.ActiveDocumentKey;
        IReadOnlyCollection<ButterMorphLanguageMappingProfile> profiles = CreateProfiles(definition, loadResult, activeMappingKey);
        ButterMorphLanguageMappingProfile activeProfile = ResolveActiveProfile(
            profiles,
            activeMappingKey);
        IButterMorphLanguageAdapter activeAdapter = ResolveAdapter(activeProfile.LanguageKey) ?? languageAdapters.First();
        ButterMorphCodeGenerationResult generationResult = activeAdapter.GenerateCode(definition, RawJson, activeProfile);

        return JsonSerializer.Serialize(new
        {
            languages = profiles.Select(profile => new
            {
                key = profile.Id,
                displayName = profile.Name,
                languageKey = profile.LanguageKey,
                exportFileExtensions = NormalizeExportFileExtensions(ResolveAdapter(profile.LanguageKey))
            }),
            profiles,
            activeProfileId = activeProfile.Id,
            result = generationResult
        }, ViewerJsonOptions);
    }

    private async Task<ButterMorphLanguageTypeMappingLoadResult> LoadTypeMappings()
    {
        foreach (IButterMorphLanguageTypeMappingHost host in typeMappingHosts)
        {
            return await host.Load(new ButterMorphLanguageTypeMappingLoadRequest
            {
                LanguageKey = string.Empty
            }) ?? new ButterMorphLanguageTypeMappingLoadResult();
        }

        return new ButterMorphLanguageTypeMappingLoadResult();
    }

    private IReadOnlyCollection<ButterMorphLanguageMappingProfile> CreateProfiles(
        PayloadSchemaDefinition definition,
        ButterMorphLanguageTypeMappingLoadResult loadResult,
        string activeMappingKey)
    {
        List<ButterMorphLanguageMappingProfile> profiles = [];
        IReadOnlyCollection<ButterMorphLanguageTypeMappingDocument> documents = ResolveDocuments(loadResult);
        bool hasHostDocuments = documents.Count > 0;
        bool hasHostProfiles = loadResult.Profiles.Count > 0;
        foreach (IButterMorphLanguageAdapter adapter in languageAdapters)
        {
            ButterMorphLanguageTypeMappingDocument adapterDocument = ResolveDocumentForLanguage(documents, adapter.Key, activeMappingKey);
            if (adapterDocument != null)
            {
                ButterMorphLanguageTypeMappingDocumentConverter.Normalize(adapterDocument);
                profiles.Add(PrepareViewerProfile(
                    ButterMorphLanguageTypeMappingDocumentConverter.ToProfile(
                        adapterDocument,
                        adapter.CreateDefaultProfile(definition, RawJson)),
                    adapter));

                continue;
            }

            ButterMorphLanguageMappingProfile adapterProfile = hasHostDocuments
                ? null
                : ResolveProfileForLanguage(loadResult.Profiles, adapter.Key, activeMappingKey);
            if (adapterProfile != null)
            {
                profiles.Add(PrepareViewerProfile(
                    MergeWithDefaultProfile(adapter.CreateDefaultProfile(definition, RawJson), adapterProfile),
                    adapter));
            }
            else if (!hasHostDocuments && !hasHostProfiles)
            {
                profiles.Add(PrepareViewerProfile(adapter.CreateDefaultProfile(definition, RawJson), adapter));
            }
        }

        if (profiles.Count == 0)
        {
            IButterMorphLanguageAdapter fallbackAdapter = languageAdapters.First();
            profiles.Add(PrepareViewerProfile(fallbackAdapter.CreateDefaultProfile(definition, RawJson), fallbackAdapter));
        }

        return profiles;
    }

    private static ButterMorphLanguageTypeMappingDocument ResolveDocumentForLanguage(
        IEnumerable<ButterMorphLanguageTypeMappingDocument> documents,
        string languageKey,
        string activeMappingKey)
    {
        ButterMorphLanguageTypeMappingDocument[] languageDocuments = documents
            .Where(document => string.Equals(document.Language, languageKey, StringComparison.OrdinalIgnoreCase))
            .ToArray();

        return languageDocuments.FirstOrDefault(document => string.Equals(document.Key, activeMappingKey, StringComparison.OrdinalIgnoreCase))
            ?? languageDocuments.FirstOrDefault();
    }

    private static ButterMorphLanguageMappingProfile ResolveProfileForLanguage(
        IEnumerable<ButterMorphLanguageMappingProfile> profiles,
        string languageKey,
        string activeMappingKey)
    {
        ButterMorphLanguageMappingProfile[] languageProfiles = profiles
            .Where(profile => string.Equals(ResolveProfileLanguage(profile), languageKey, StringComparison.OrdinalIgnoreCase))
            .ToArray();

        return languageProfiles.FirstOrDefault(profile =>
                string.Equals(profile.Id, activeMappingKey, StringComparison.OrdinalIgnoreCase)
                || string.Equals(profile.Key, activeMappingKey, StringComparison.OrdinalIgnoreCase))
            ?? languageProfiles.FirstOrDefault();
    }

    private static ButterMorphLanguageMappingProfile PrepareViewerProfile(
        ButterMorphLanguageMappingProfile profile,
        IButterMorphLanguageAdapter adapter)
    {
        profile.Name = adapter.DisplayName;
        profile.LanguageKey = adapter.Key;
        profile.Language = adapter.Key;
        return profile;
    }

    private static IReadOnlyCollection<ButterMorphLanguageTypeMappingDocument> ResolveDocuments(ButterMorphLanguageTypeMappingLoadResult loadResult)
    {
        if (loadResult.Documents.Count > 0)
        {
            return loadResult.Documents.ToArray();
        }

        return loadResult.Profiles
            .Select(ButterMorphLanguageTypeMappingDocumentConverter.FromProfile)
            .ToArray();
    }

    private static ButterMorphLanguageMappingProfile MergeWithDefaultProfile(ButterMorphLanguageMappingProfile defaultProfile, ButterMorphLanguageMappingProfile hostProfile)
    {
        if (hostProfile == null)
        {
            return defaultProfile;
        }

        Dictionary<string, ButterMorphLanguageTypeMapping> typeMappings = defaultProfile.TypeMappings
            .Where(mapping => !string.IsNullOrWhiteSpace(mapping.ButterMorphType))
            .ToDictionary(mapping => CreateMappingKey(mapping.ButterMorphType, mapping.ButterMorphTypeVersion), StringComparer.OrdinalIgnoreCase);
        foreach (ButterMorphLanguageTypeMapping mapping in hostProfile.TypeMappings)
        {
            if (!string.IsNullOrWhiteSpace(mapping.ButterMorphType))
            {
                typeMappings[CreateMappingKey(mapping.ButterMorphType, mapping.ButterMorphTypeVersion)] = mapping;
            }
        }

        hostProfile.TypeMappings = typeMappings.Values
            .OrderBy(mapping => mapping.ButterMorphType, StringComparer.OrdinalIgnoreCase)
            .ThenBy(mapping => mapping.ButterMorphTypeVersion, StringComparer.OrdinalIgnoreCase)
            .ToList();
        hostProfile.Id = string.IsNullOrWhiteSpace(hostProfile.Id) ? defaultProfile.Id : hostProfile.Id;
        hostProfile.Key = string.IsNullOrWhiteSpace(hostProfile.Key) ? defaultProfile.Key : hostProfile.Key;
        hostProfile.Name = string.IsNullOrWhiteSpace(hostProfile.Name) ? defaultProfile.Name : hostProfile.Name;
        hostProfile.Description = string.IsNullOrWhiteSpace(hostProfile.Description) ? defaultProfile.Description : hostProfile.Description;
        hostProfile.LanguageKey = string.IsNullOrWhiteSpace(hostProfile.LanguageKey) ? defaultProfile.LanguageKey : hostProfile.LanguageKey;
        hostProfile.Language = string.IsNullOrWhiteSpace(hostProfile.Language) ? defaultProfile.Language : hostProfile.Language;
        hostProfile.Namespace = string.IsNullOrWhiteSpace(hostProfile.Namespace) ? defaultProfile.Namespace : hostProfile.Namespace;
        hostProfile.RootTypeName = string.IsNullOrWhiteSpace(hostProfile.RootTypeName) ? defaultProfile.RootTypeName : hostProfile.RootTypeName;
        hostProfile.CollectionType = string.IsNullOrWhiteSpace(hostProfile.CollectionType) ? defaultProfile.CollectionType : hostProfile.CollectionType;
        return hostProfile;
    }

    private static string CreateMappingKey(string type, string version)
    {
        return (type ?? string.Empty) + "@" + (version ?? string.Empty);
    }

    private static string ResolveProfileLanguage(ButterMorphLanguageMappingProfile profile)
    {
        return string.IsNullOrWhiteSpace(profile.LanguageKey) ? profile.Language : profile.LanguageKey;
    }

    private static ButterMorphLanguageMappingProfile ResolveActiveProfile(IReadOnlyCollection<ButterMorphLanguageMappingProfile> profiles, string activeProfileId)
    {
        return profiles.FirstOrDefault(profile => string.Equals(profile.Id, activeProfileId, StringComparison.OrdinalIgnoreCase))
            ?? profiles.First();
    }

    private async Task<ButterMorphLanguageMappingProfile> ReadProfile()
    {
        return await JsonSerializer.DeserializeAsync<ButterMorphLanguageMappingProfile>(Request.Body, ViewerJsonOptions)
            ?? new ButterMorphLanguageMappingProfile();
    }

    private IButterMorphLanguageAdapter ResolveAdapter(string languageKey)
    {
        return languageAdapters.FirstOrDefault(adapter => string.Equals(adapter.Key, languageKey, StringComparison.OrdinalIgnoreCase));
    }

    private static IReadOnlyCollection<string> NormalizeExportFileExtensions(IButterMorphLanguageAdapter adapter)
    {
        return (adapter?.ExportFileExtensions ?? [])
            .Where(extension => !string.IsNullOrWhiteSpace(extension))
            .Select(extension => extension.Trim().StartsWith(".", StringComparison.Ordinal)
                ? extension.Trim()
                : "." + extension.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    private static string CreateRawJson(ButterMorphPayloadSchemaViewerLoadResult result)
    {
        if (!string.IsNullOrWhiteSpace(result.JsonSchema))
        {
            return PrettyJson(result.JsonSchema);
        }

        if (result.Definition != null)
        {
            return JsonSerializer.Serialize(result.Definition, PrettyJsonOptions);
        }

        return "{}";
    }

    private string ResolveActiveView()
    {
        string requestedView = Request.Query["view"].ToString();
        if (string.Equals(requestedView, "json", StringComparison.OrdinalIgnoreCase) && JsonEnabled)
        {
            return "json";
        }

        if (string.Equals(requestedView, "code", StringComparison.OrdinalIgnoreCase) && CodeEnabled)
        {
            return "code";
        }

        if (string.Equals(requestedView, "ux", StringComparison.OrdinalIgnoreCase) && UserFriendlyEnabled)
        {
            return "ux";
        }

        if (options.SchemaViewer.DefaultView == ButterMorphSchemaViewerView.Json && JsonEnabled)
        {
            return "json";
        }

        if (options.SchemaViewer.DefaultView == ButterMorphSchemaViewerView.Code && CodeEnabled)
        {
            return "code";
        }

        if (UserFriendlyEnabled)
        {
            return "ux";
        }

        return JsonEnabled ? "json" : CodeEnabled ? "code" : "none";
    }

    private bool HasView(ButterMorphSchemaViewerViews view)
    {
        return (options.SchemaViewer.EnabledViews & view) == view;
    }

    private int EnabledViewCount => Convert.ToInt32(UserFriendlyEnabled) + Convert.ToInt32(JsonEnabled) + Convert.ToInt32(CodeEnabled);

    private static string PrettyJson(string json)
    {
        try
        {
            using JsonDocument document = JsonDocument.Parse(json);
            return JsonSerializer.Serialize(document.RootElement, PrettyJsonOptions);
        }
        catch (JsonException)
        {
            return json;
        }
    }
}
