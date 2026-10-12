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
    /// Prepares one language type mapping document for the host.
    /// </summary>
    /// <returns>The save response.</returns>
    public async Task<IActionResult> OnPostSaveDocument()
    {
        ButterMorphLanguageTypeMappingDocument document = await JsonSerializer.DeserializeAsync<ButterMorphLanguageTypeMappingDocument>(Request.Body, JsonOptions)
            ?? new ButterMorphLanguageTypeMappingDocument();
        ButterMorphLanguageTypeMappingDocumentConverter.Normalize(document);

        return new JsonResult(new
        {
            succeeded = true,
            document,
            message = "Language type mapping document is ready for the host."
        }, JsonOptions);
    }

    /// <summary>
    /// Prepares one legacy language type mapping profile for the host.
    /// </summary>
    /// <returns>The save response.</returns>
    public async Task<IActionResult> OnPostSaveProfile()
    {
        ButterMorphLanguageMappingProfile profile = await JsonSerializer.DeserializeAsync<ButterMorphLanguageMappingProfile>(Request.Body, JsonOptions)
            ?? new ButterMorphLanguageMappingProfile();
        ButterMorphLanguageTypeMappingDocument document = ButterMorphLanguageTypeMappingDocumentConverter.FromProfile(profile);
        ButterMorphLanguageTypeMappingDocumentConverter.Normalize(document);

        return new JsonResult(new
        {
            succeeded = true,
            document,
            message = "Language type mapping document is ready for the host."
        }, JsonOptions);
    }

    private async Task<string> CreateMappingDataJson()
    {
        string contextKey = ResolveContextKey();
        ButterMorphLanguageTypeMappingLoadResult loadResult = await LoadDocuments(contextKey);
        IReadOnlyCollection<ButterMorphTypeMappingCatalogItem> butterMorphTypes = loadResult.ButterMorphTypes.Count > 0
            ? loadResult.ButterMorphTypes
            : CreateBaseTypeCatalog();
        IReadOnlyCollection<ButterMorphLanguageTypeMappingDocument> documents = CreateDocuments(loadResult.Documents, loadResult.Profiles, contextKey);
        string activeDocumentKey = FirstNonEmpty(loadResult.ActiveDocumentKey, loadResult.ActiveProfileId, contextKey);
        if (!string.IsNullOrWhiteSpace(contextKey) &&
            documents.All(document => !string.Equals(document.Key, contextKey, StringComparison.OrdinalIgnoreCase)))
        {
            activeDocumentKey = documents.FirstOrDefault(document => string.IsNullOrWhiteSpace(document.Key))?.Key ?? activeDocumentKey;
        }

        ButterMorphLanguageTypeMappingDocument activeDocument = ResolveActiveDocument(
            documents,
            activeDocumentKey);
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
            documents,
            bundles = loadResult.Bundles,
            activeDocumentKey = activeDocument?.Key ?? string.Empty
        }, JsonOptions);
    }

    private async Task<ButterMorphLanguageTypeMappingLoadResult> LoadDocuments(string contextKey)
    {
        foreach (IButterMorphLanguageTypeMappingHost host in hosts)
        {
            return await host.Load(new ButterMorphLanguageTypeMappingLoadRequest
            {
                ContextKey = contextKey
            }) ?? new ButterMorphLanguageTypeMappingLoadResult();
        }

        return new ButterMorphLanguageTypeMappingLoadResult();
    }

    private IReadOnlyCollection<ButterMorphLanguageTypeMappingDocument> CreateDocuments(
        IReadOnlyCollection<ButterMorphLanguageTypeMappingDocument> hostDocuments,
        IReadOnlyCollection<ButterMorphLanguageMappingProfile> hostProfiles,
        string contextKey)
    {
        List<ButterMorphLanguageTypeMappingDocument> documents = [];
        foreach (ButterMorphLanguageTypeMappingDocument document in hostDocuments ?? [])
        {
            ButterMorphLanguageTypeMappingDocumentConverter.Normalize(document);
            documents.Add(document);
        }

        if (documents.Count == 0)
        {
            foreach (ButterMorphLanguageMappingProfile profile in hostProfiles ?? [])
            {
                ButterMorphLanguageTypeMappingDocument document = ButterMorphLanguageTypeMappingDocumentConverter.FromProfile(profile);
                ButterMorphLanguageTypeMappingDocumentConverter.Normalize(document);
                documents.Add(document);
            }
        }

        if (!string.IsNullOrWhiteSpace(contextKey) &&
            documents.All(document => !string.Equals(document.Key, contextKey, StringComparison.OrdinalIgnoreCase)))
        {
            IButterMorphLanguageAdapter adapter = languageAdapters.FirstOrDefault();
            if (adapter != null)
            {
                documents.Add(CreateEmptyDocument(adapter));
            }
        }

        if (documents.Count == 0)
        {
            IButterMorphLanguageAdapter adapter = languageAdapters.FirstOrDefault();
            if (adapter != null)
            {
                documents.Add(CreateEmptyDocument(adapter));
            }
        }

        return documents
            .OrderBy(document => document.Language, StringComparer.OrdinalIgnoreCase)
            .ThenBy(document => document.Name, StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    private static ButterMorphLanguageTypeMappingDocument CreateEmptyDocument(IButterMorphLanguageAdapter adapter)
    {
        return new ButterMorphLanguageTypeMappingDocument
        {
            Key = string.Empty,
            Name = string.Empty,
            Description = string.Empty,
            Language = adapter.Key
        };
    }

    private static ButterMorphLanguageTypeMappingDocument ResolveActiveDocument(
        IReadOnlyCollection<ButterMorphLanguageTypeMappingDocument> documents,
        string activeDocumentKey)
    {
        return documents.FirstOrDefault(document => string.Equals(document.Key, activeDocumentKey, StringComparison.OrdinalIgnoreCase))
            ?? documents.FirstOrDefault();
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

    private string ResolveContextKey()
    {
        return DesignerSessionKeyResolver.ResolveContextKey(this, options);
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
