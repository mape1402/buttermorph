namespace ButterMorph.Web.Razor;

using System.Text.Json;
using ButterMorph.SchemaDesign;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Options;

/// <summary>
/// Displays the reusable field metadata designer.
/// </summary>
public sealed class MetadataFieldsDesignerModel : PageModel
{
    // Builds metadata output.
    private readonly IFieldMetadataDefinitionBuilder metadataBuilder;

    // Reads designer integration options.
    private readonly ButterMorphRazorDesignerOptions options;

    // Provides optional host integrations.
    private readonly IEnumerable<IButterMorphFieldMetadataDesignerHost> hosts;

    /// <summary>
    /// Initializes a new instance of the <see cref="MetadataFieldsDesignerModel"/> class.
    /// </summary>
    /// <param name="metadataBuilder">The metadata builder.</param>
    /// <param name="options">The designer options.</param>
    /// <param name="hosts">The optional host integrations.</param>
    public MetadataFieldsDesignerModel(
        IFieldMetadataDefinitionBuilder metadataBuilder,
        IOptions<ButterMorphRazorDesignerOptions> options,
        IEnumerable<IButterMorphFieldMetadataDesignerHost> hosts)
    {
        this.metadataBuilder = metadataBuilder;
        this.options = options.Value;
        this.hosts = hosts;
    }

    /// <summary>
    /// Gets or sets the editable metadata input.
    /// </summary>
    [BindProperty]
    public FieldMetadataDesignInput Input { get; set; } = new();

    /// <summary>
    /// Gets or sets validation JSON.
    /// </summary>
    public string ValidationJson { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets applies-to JSON.
    /// </summary>
    public string AppliesToJson { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a value indicating whether manual actions are shown.
    /// </summary>
    public bool ShowManualActions { get; set; } = true;

    /// <summary>
    /// Gets or sets a user-facing message.
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a value indicating whether host save completed.
    /// </summary>
    public bool HostSaveCompleted { get; set; }

    /// <summary>
    /// Gets or sets the saved host context key.
    /// </summary>
    public string SavedContextKey { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the form title.
    /// </summary>
    public string FormTitle { get; set; } = "Nuevo custom field";

    /// <summary>
    /// Gets or sets the form save URL.
    /// </summary>
    public string SaveActionUrl { get; set; } = string.Empty;

    /// <summary>
    /// Handles initial display.
    /// </summary>
    /// <returns>The page result.</returns>
    public async Task<IActionResult> OnGet()
    {
        SaveActionUrl = ResolveSaveActionUrl();
        FormTitle = ResolveFormTitle();
        await ApplyHostLoad();
        RefreshPreview();

        return Page();
    }

    /// <summary>
    /// Redirects accidental save GET requests back to the host.
    /// </summary>
    /// <returns>The redirect result.</returns>
    public IActionResult OnGetSave()
    {
        return BadRequest("Schema designer save requires POST.");
    }

    /// <summary>
    /// Saves metadata state through the optional host.
    /// </summary>
    /// <returns>The page result.</returns>
    public async Task<IActionResult> OnPostSave()
    {
        SaveActionUrl = ResolveSaveActionUrl();
        FormTitle = ResolveFormTitle();
        FieldMetadataDesignResult result = metadataBuilder.Build(Input);
        ValidationJson = result.ValidationJson;
        AppliesToJson = result.AppliesToJson;

        if (!result.Succeeded)
        {
            Message = string.Join(" ", result.Diagnostics.Select(diagnostic => diagnostic.Message));
            if (IsPopupRequest())
            {
                return new JsonResult(CreateHostSaveResponse("ButterMorphFieldMetadataDesignerSaved"));
            }

            return Page();
        }

        ButterMorphFieldMetadataDesignerSaveResult saveResult = new()
        {
            Succeeded = true,
            Message = "Metadata field saved."
        };

        foreach (IButterMorphFieldMetadataDesignerHost host in hosts)
        {
            saveResult = await host.Save(new ButterMorphFieldMetadataDesignerSaveRequest
            {
                ContextKey = ResolveContextKey(),
                Result = result
            });
            break;
        }

        HostSaveCompleted = saveResult.Succeeded;
        SavedContextKey = ResolveContextKey();
        if (saveResult.Succeeded && IsPopupRequest())
        {
            return new JsonResult(CreateHostSaveResponse("ButterMorphFieldMetadataDesignerSaved"));
        }

        if (IsPopupRequest())
        {
            Message = saveResult.Message;
            return new JsonResult(CreateHostSaveResponse("ButterMorphFieldMetadataDesignerSaved"));
        }

        if (saveResult.Succeeded)
        {
            Message = saveResult.Message;
            return Page();
        }

        return Page();
    }

    // Applies host preload to the page.
    private async Task ApplyHostLoad()
    {
        foreach (IButterMorphFieldMetadataDesignerHost host in hosts)
        {
            ButterMorphFieldMetadataDesignerLoadResult result = await host.Load(new ButterMorphFieldMetadataDesignerLoadRequest
            {
                ContextKey = ResolveContextKey()
            });
            Input = result.Input;
            ShowManualActions = result.ShowManualActions;
            Message = result.Message;
            return;
        }
    }

    // Refreshes JSON previews.
    private void RefreshPreview()
    {
        FieldMetadataDesignResult result = metadataBuilder.Build(Input);
        ValidationJson = result.ValidationJson;
        AppliesToJson = result.AppliesToJson;
    }

    // Resolves the host context key.
    private string ResolveContextKey()
    {
        return DesignerSessionKeyResolver.ResolveContextKey(this, options);
    }

    // Resolves the form action while preserving host flow query parameters.
    private string ResolveSaveActionUrl()
    {
        string path = Request.Path.ToString();
        string query = Request.QueryString.ToString();
        if (query.Contains("handler=", StringComparison.OrdinalIgnoreCase))
        {
            return path + query;
        }

        string separator = "&";
        if (string.IsNullOrEmpty(query))
        {
            separator = Convert.ToChar(63).ToString();
        }

        return path + query + separator + "handler=Save";
    }

    // Creates the host flow save response.
    private SchemaDesignerHostSaveResponse CreateHostSaveResponse(string messageType)
    {
        return new SchemaDesignerHostSaveResponse
        {
            HostSaveCompleted = HostSaveCompleted,
            SavedContextKey = ResolveContextKey(),
            MessageType = messageType,
            Message = Message
        };
    }

    // Detects popup-host requests.
    private bool IsPopupRequest()
    {
        return string.Equals(Request.Query[options.PopupQueryParameter].ToString(), "true", StringComparison.OrdinalIgnoreCase);
    }

    // Resolves the title based on popup mode.
    private string ResolveFormTitle()
    {
        string mode = Request.Query["mode"].ToString();
        if (string.Equals(mode, "edit", StringComparison.OrdinalIgnoreCase))
        {
            return "Editar custom field";
        }

        return "Nuevo custom field";
    }
}
