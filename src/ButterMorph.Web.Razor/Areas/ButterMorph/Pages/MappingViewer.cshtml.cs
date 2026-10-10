namespace ButterMorph.Web.Razor;

using System.Text.Json;
using ButterMorph.Abstractions;
using ButterMorph.Core;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Options;

/// <summary>
/// Displays a reusable read-only mapping document viewer.
/// </summary>
public sealed class MappingViewerModel : PageModel
{
    private static readonly JsonSerializerOptions ViewerJsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    private readonly ButterMorphRazorDesignerOptions options;
    private readonly IEnumerable<IButterMorphMappingViewerHost> hosts;
    private readonly IDslExporter dslExporter;

    /// <summary>
    /// Initializes a new instance of the <see cref="MappingViewerModel"/> class.
    /// </summary>
    /// <param name="options">The reusable designer options.</param>
    /// <param name="hosts">The optional mapping viewer hosts.</param>
    /// <param name="dslExporter">The mapping DSL exporter.</param>
    public MappingViewerModel(
        IOptions<ButterMorphRazorDesignerOptions> options,
        IEnumerable<IButterMorphMappingViewerHost> hosts,
        IDslExporter dslExporter)
    {
        this.options = options.Value;
        this.hosts = hosts;
        this.dslExporter = dslExporter;
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
    /// Gets the rendered mapping display model.
    /// </summary>
    internal DocumentViewerDisplayModel Viewer { get; private set; } = new();

    /// <summary>
    /// Gets the serialized mapping display model for client interaction.
    /// </summary>
    public string ViewerDataJson { get; private set; } = "{}";

    /// <summary>
    /// Gets the raw mapping DSL.
    /// </summary>
    public string RawDsl { get; private set; } = string.Empty;

    /// <summary>
    /// Gets a user-facing message.
    /// </summary>
    public string Message { get; private set; } = string.Empty;

    /// <summary>
    /// Gets a value indicating whether the user-friendly viewer is enabled.
    /// </summary>
    public bool UserFriendlyEnabled => HasView(ButterMorphDocumentViewerViews.UserFriendly);

    /// <summary>
    /// Gets a value indicating whether the raw DSL viewer is enabled.
    /// </summary>
    public bool DslEnabled => HasView(ButterMorphDocumentViewerViews.Dsl);

    /// <summary>
    /// Gets a value indicating whether the view switch should be shown.
    /// </summary>
    public bool ShowViewSwitch => UserFriendlyEnabled && DslEnabled;

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
        ButterMorphMappingViewerLoadResult result = await LoadMapping();
        result.Document ??= new TransformationDocument();
        result.TargetSchema ??= result.Document.TargetSchema;
        RawDsl = CreateRawDsl(result);
        Viewer = DocumentViewerDisplayBuilder.BuildMapping(result);
        ViewerDataJson = JsonSerializer.Serialize(Viewer, ViewerJsonOptions);
        Message = result.Message;
        ActiveView = ResolveActiveView();
    }

    private async Task<ButterMorphMappingViewerLoadResult> LoadMapping()
    {
        foreach (IButterMorphMappingViewerHost host in hosts)
        {
            ButterMorphMappingViewerLoadResult result = await host.Load(new ButterMorphMappingViewerLoadRequest
            {
                ContextKey = DesignerSessionKeyResolver.ResolveContextKey(this, options)
            });

            return result ?? new ButterMorphMappingViewerLoadResult();
        }

        return new ButterMorphMappingViewerLoadResult
        {
            Message = "No mapping viewer host is configured."
        };
    }

    private string CreateRawDsl(ButterMorphMappingViewerLoadResult result)
    {
        if (!string.IsNullOrWhiteSpace(result.DslContent))
        {
            return result.DslContent;
        }

        return result.Document == null ? string.Empty : dslExporter.Export(result.Document);
    }

    private string ResolveActiveView()
    {
        string requestedView = Request.Query["view"].ToString();
        if (string.Equals(requestedView, "dsl", StringComparison.OrdinalIgnoreCase) && DslEnabled)
        {
            return "dsl";
        }

        if (string.Equals(requestedView, "ux", StringComparison.OrdinalIgnoreCase) && UserFriendlyEnabled)
        {
            return "ux";
        }

        if (options.MappingViewer.DefaultView == ButterMorphDocumentViewerView.Dsl && DslEnabled)
        {
            return "dsl";
        }

        if (UserFriendlyEnabled)
        {
            return "ux";
        }

        return DslEnabled ? "dsl" : "none";
    }

    private bool HasView(ButterMorphDocumentViewerViews view)
    {
        return (options.MappingViewer.EnabledViews & view) == view;
    }
}
