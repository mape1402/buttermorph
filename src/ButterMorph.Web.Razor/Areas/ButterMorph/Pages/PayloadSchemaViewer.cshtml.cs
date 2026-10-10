namespace ButterMorph.Web.Razor;

using System.Text.Json;
using ButterMorph.SchemaDesign;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Options;

/// <summary>
/// Displays a reusable read-only payload schema viewer.
/// </summary>
public sealed class PayloadSchemaViewerModel : PageModel
{
    private static readonly JsonSerializerOptions PrettyJsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true
    };

    private static readonly JsonSerializerOptions ViewerJsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    private readonly ButterMorphRazorDesignerOptions options;
    private readonly IEnumerable<IButterMorphPayloadSchemaViewerHost> hosts;

    /// <summary>
    /// Initializes a new instance of the <see cref="PayloadSchemaViewerModel"/> class.
    /// </summary>
    /// <param name="options">The reusable designer options.</param>
    /// <param name="hosts">The optional schema viewer hosts.</param>
    public PayloadSchemaViewerModel(
        IOptions<ButterMorphRazorDesignerOptions> options,
        IEnumerable<IButterMorphPayloadSchemaViewerHost> hosts)
    {
        this.options = options.Value;
        this.hosts = hosts;
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
    /// Gets a value indicating whether the view switch should be shown.
    /// </summary>
    public bool ShowViewSwitch => UserFriendlyEnabled && JsonEnabled;

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
        Viewer = SchemaViewerDisplayBuilder.Build(result.Definition, RawJson);
        ViewerDataJson = JsonSerializer.Serialize(Viewer, ViewerJsonOptions);
        Message = result.Message;
        ActiveView = ResolveActiveView();
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

        if (string.Equals(requestedView, "ux", StringComparison.OrdinalIgnoreCase) && UserFriendlyEnabled)
        {
            return "ux";
        }

        if (options.SchemaViewer.DefaultView == ButterMorphSchemaViewerView.Json && JsonEnabled)
        {
            return "json";
        }

        if (UserFriendlyEnabled)
        {
            return "ux";
        }

        return JsonEnabled ? "json" : "none";
    }

    private bool HasView(ButterMorphSchemaViewerViews view)
    {
        return (options.SchemaViewer.EnabledViews & view) == view;
    }

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
