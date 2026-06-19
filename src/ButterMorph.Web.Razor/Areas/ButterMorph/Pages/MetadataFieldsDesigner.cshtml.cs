namespace ButterMorph.Web.Razor;

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
    /// Gets or sets a safe local return URL.
    /// </summary>
    public string SafeReturnUrl { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the form title.
    /// </summary>
    public string FormTitle { get; set; } = "Nuevo custom field";

    /// <summary>
    /// Handles initial display.
    /// </summary>
    /// <returns>The page result.</returns>
    public async Task<IActionResult> OnGet()
    {
        FormTitle = ResolveFormTitle();
        await ApplyHostLoad();
        RefreshPreview();

        return Page();
    }

    /// <summary>
    /// Saves metadata state through the optional host.
    /// </summary>
    /// <returns>The page result.</returns>
    public async Task<IActionResult> OnPostSave()
    {
        FormTitle = ResolveFormTitle();
        FieldMetadataDesignResult result = metadataBuilder.Build(Input);
        ValidationJson = result.ValidationJson;
        AppliesToJson = result.AppliesToJson;

        if (!result.Succeeded)
        {
            Message = string.Join(" ", result.Diagnostics.Select(diagnostic => diagnostic.Message));
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

        Message = saveResult.Message;
        HostSaveCompleted = saveResult.Succeeded;
        SafeReturnUrl = ResolveSafeReturnUrl();

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

    // Resolves a safe return URL.
    private string ResolveSafeReturnUrl()
    {
        string value = Request.Query[options.ReturnUrlQueryParameter].ToString();
        if (!string.IsNullOrWhiteSpace(value) && Url.IsLocalUrl(value))
        {
            return value;
        }

        return string.Empty;
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
