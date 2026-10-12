namespace ButterMorph.Web.Razor;

using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Options;

/// <summary>
/// Displays a reusable language type bundle designer.
/// </summary>
[IgnoreAntiforgeryToken]
public sealed class LanguageTypeBundlesDesignerModel : PageModel
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        WriteIndented = true
    };

    private readonly ButterMorphRazorDesignerOptions options;
    private readonly IReadOnlyCollection<IButterMorphLanguageAdapter> languageAdapters;
    private readonly IButterMorphLanguageTypeBundleDiscovery discovery;

    /// <summary>
    /// Initializes a new instance of the <see cref="LanguageTypeBundlesDesignerModel"/> class.
    /// </summary>
    /// <param name="options">The reusable designer options.</param>
    /// <param name="languageAdapters">The language adapters.</param>
    /// <param name="discovery">The bundle discovery service.</param>
    public LanguageTypeBundlesDesignerModel(
        IOptions<ButterMorphRazorDesignerOptions> options,
        IEnumerable<IButterMorphLanguageAdapter> languageAdapters,
        IButterMorphLanguageTypeBundleDiscovery discovery)
    {
        this.options = options.Value;
        this.languageAdapters = languageAdapters
            .OrderBy(adapter => adapter.DisplayName, StringComparer.OrdinalIgnoreCase)
            .ToArray();
        this.discovery = discovery;
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
    /// Gets the serialized bundle data.
    /// </summary>
    public string BundleDataJson { get; private set; } = "{}";

    /// <summary>
    /// Handles initial display.
    /// </summary>
    public void OnGet()
    {
        BundleDataJson = JsonSerializer.Serialize(new
        {
            languages = languageAdapters.Select(adapter => new
            {
                adapter.Key,
                adapter.DisplayName
            })
        }, JsonOptions);
    }

    /// <summary>
    /// Discovers bundle metadata from one uploaded assembly.
    /// </summary>
    /// <returns>The discovery response.</returns>
    public IActionResult OnPostDiscoverBundle()
    {
        IFormFile file = Request.Form.Files.FirstOrDefault();
        if (file == null)
        {
            return new JsonResult(new ButterMorphLanguageTypeBundleDiscoveryResult
            {
                Succeeded = false,
                Diagnostics = ["Assembly file is required."]
            }, JsonOptions);
        }

        using Stream stream = file.OpenReadStream();
        ButterMorphLanguageTypeBundleDiscoveryResult result = discovery.Discover(stream, file.FileName);
        return new JsonResult(result, JsonOptions);
    }

    /// <summary>
    /// Publishes one discovered bundle document for the host.
    /// </summary>
    /// <returns>The publish response.</returns>
    public async Task<IActionResult> OnPostSaveBundle()
    {
        ButterMorphLanguageTypeBundle bundle = await JsonSerializer.DeserializeAsync<ButterMorphLanguageTypeBundle>(Request.Body, JsonOptions)
            ?? new ButterMorphLanguageTypeBundle();
        if (string.IsNullOrWhiteSpace(bundle.AssemblyKey) && string.IsNullOrWhiteSpace(bundle.AssemblyId))
        {
            return new JsonResult(new ButterMorphLanguageTypeBundleSaveResult
            {
                Succeeded = false,
                Bundle = bundle,
                Message = "A discovered bundle is required."
            }, JsonOptions);
        }

        return new JsonResult(new ButterMorphLanguageTypeBundleSaveResult
        {
            Succeeded = true,
            Bundle = bundle,
            Message = "Bundle document is ready for the host."
        }, JsonOptions);
    }
}
