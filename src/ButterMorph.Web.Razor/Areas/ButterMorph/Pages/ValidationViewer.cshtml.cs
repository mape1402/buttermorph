namespace ButterMorph.Web.Razor;

using System.Text.Json;
using ButterMorph.Abstractions;
using ButterMorph.Core;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Options;

/// <summary>
/// Displays a reusable read-only validation document viewer.
/// </summary>
public sealed class ValidationViewerModel : PageModel
{
    private static readonly JsonSerializerOptions ViewerJsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    private readonly ButterMorphRazorDesignerOptions options;
    private readonly IEnumerable<IButterMorphValidationViewerHost> hosts;
    private readonly IValidationDslExporter dslExporter;

    /// <summary>
    /// Initializes a new instance of the <see cref="ValidationViewerModel"/> class.
    /// </summary>
    /// <param name="options">The reusable designer options.</param>
    /// <param name="hosts">The optional validation viewer hosts.</param>
    /// <param name="dslExporter">The validation DSL exporter.</param>
    public ValidationViewerModel(
        IOptions<ButterMorphRazorDesignerOptions> options,
        IEnumerable<IButterMorphValidationViewerHost> hosts,
        IValidationDslExporter dslExporter)
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
    /// Gets the rendered validation display model.
    /// </summary>
    internal DocumentViewerDisplayModel Viewer { get; private set; } = new();

    /// <summary>
    /// Gets the serialized validation display model for client interaction.
    /// </summary>
    public string ViewerDataJson { get; private set; } = "{}";

    /// <summary>
    /// Gets the raw validation DSL.
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
        ButterMorphValidationViewerLoadResult result = await LoadValidation();
        result.Document ??= new ValidationDocument();
        RawDsl = CreateRawDsl(result);
        Viewer = DocumentViewerDisplayBuilder.BuildValidation(result);
        ViewerDataJson = JsonSerializer.Serialize(Viewer, ViewerJsonOptions);
        Message = result.Message;
        ActiveView = ResolveActiveView();
    }

    private async Task<ButterMorphValidationViewerLoadResult> LoadValidation()
    {
        foreach (IButterMorphValidationViewerHost host in hosts)
        {
            ButterMorphValidationViewerLoadResult result = await host.Load(new ButterMorphValidationViewerLoadRequest
            {
                ContextKey = DesignerSessionKeyResolver.ResolveContextKey(this, options)
            });

            return result ?? new ButterMorphValidationViewerLoadResult();
        }

        return new ButterMorphValidationViewerLoadResult
        {
            Message = "No validation viewer host is configured."
        };
    }

    private string CreateRawDsl(ButterMorphValidationViewerLoadResult result)
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

        if (options.ValidationViewer.DefaultView == ButterMorphDocumentViewerView.Dsl && DslEnabled)
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
        return (options.ValidationViewer.EnabledViews & view) == view;
    }
}
