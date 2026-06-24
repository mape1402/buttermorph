namespace ButterMorph.Web.Razor;

using System.Text.Json;
using ButterMorph.SchemaDesign;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Options;

/// <summary>
/// Displays the reusable schema type designer.
/// </summary>
public sealed class SchemaTypesDesignerModel : PageModel
{
    // Builds schema type JSON Schema output.
    private readonly ISchemaTypeSchemaBuilder schemaTypeBuilder;

    // Reads designer integration options.
    private readonly ButterMorphRazorDesignerOptions options;

    // Provides optional host integrations.
    private readonly IEnumerable<IButterMorphSchemaTypeDesignerHost> hosts;

    /// <summary>
    /// Initializes a new instance of the <see cref="SchemaTypesDesignerModel"/> class.
    /// </summary>
    /// <param name="schemaTypeBuilder">The schema type builder.</param>
    /// <param name="options">The designer options.</param>
    /// <param name="hosts">The optional host integrations.</param>
    public SchemaTypesDesignerModel(
        ISchemaTypeSchemaBuilder schemaTypeBuilder,
        IOptions<ButterMorphRazorDesignerOptions> options,
        IEnumerable<IButterMorphSchemaTypeDesignerHost> hosts)
    {
        this.schemaTypeBuilder = schemaTypeBuilder;
        this.options = options.Value;
        this.hosts = hosts;
    }

    /// <summary>
    /// Gets or sets the editable schema type input.
    /// </summary>
    [BindProperty]
    public SchemaTypeDesignInput Input { get; set; } = new();

    /// <summary>
    /// Gets or sets the generated JSON Schema.
    /// </summary>
    public string JsonSchema { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets available schema types.
    /// </summary>
    public IReadOnlyCollection<SchemaTypeCatalogItem> SchemaTypes { get; set; } = [];

    /// <summary>
    /// Gets or sets catalog JSON used by client behavior.
    /// </summary>
    public string SchemaTypeCatalogJson { get; set; } = "[]";

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
    public string FormTitle { get; set; } = "Nuevo tipo personalizado";

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
        RefreshCatalog();
        JsonSchema = schemaTypeBuilder.Build(Input, SchemaTypes).JsonSchema;

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
    /// Saves schema type state through the optional host.
    /// </summary>
    /// <returns>The page result.</returns>
    public async Task<IActionResult> OnPostSave()
    {
        SaveActionUrl = ResolveSaveActionUrl();
        FormTitle = ResolveFormTitle();
        await ApplyHostLoadCatalogOnly();
        SchemaTypeDesignResult result = schemaTypeBuilder.Build(Input, SchemaTypes);
        JsonSchema = result.JsonSchema;
        RefreshCatalog();

        if (!result.Succeeded)
        {
            Message = string.Join(" ", result.Diagnostics.Select(diagnostic => diagnostic.Message));
            if (IsPopupRequest())
            {
                return new JsonResult(CreateHostSaveResponse("ButterMorphSchemaTypeDesignerSaved"));
            }

            return Page();
        }

        ButterMorphSchemaTypeDesignerSaveResult saveResult = new()
        {
            Succeeded = true,
            Message = "Schema type saved."
        };

        foreach (IButterMorphSchemaTypeDesignerHost host in hosts)
        {
            saveResult = await host.Save(new ButterMorphSchemaTypeDesignerSaveRequest
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
            return new JsonResult(CreateHostSaveResponse("ButterMorphSchemaTypeDesignerSaved"));
        }

        if (IsPopupRequest())
        {
            Message = saveResult.Message;
            return new JsonResult(CreateHostSaveResponse("ButterMorphSchemaTypeDesignerSaved"));
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
        foreach (IButterMorphSchemaTypeDesignerHost host in hosts)
        {
            ButterMorphSchemaTypeDesignerLoadResult result = await host.Load(new ButterMorphSchemaTypeDesignerLoadRequest
            {
                ContextKey = ResolveContextKey()
            });
            Input = result.Input;
            SchemaTypes = result.SchemaTypes;
            ShowManualActions = result.ShowManualActions;
            Message = result.Message;
            return;
        }

        SchemaTypes = CreateDefaultCatalog();
    }

    // Applies only host catalog data during posts.
    private async Task ApplyHostLoadCatalogOnly()
    {
        foreach (IButterMorphSchemaTypeDesignerHost host in hosts)
        {
            ButterMorphSchemaTypeDesignerLoadResult result = await host.Load(new ButterMorphSchemaTypeDesignerLoadRequest
            {
                ContextKey = ResolveContextKey()
            });
            SchemaTypes = result.SchemaTypes;
            ShowManualActions = result.ShowManualActions;
            return;
        }

        SchemaTypes = CreateDefaultCatalog();
    }

    // Refreshes serialized catalog data.
    private void RefreshCatalog()
    {
        if (SchemaTypes.Count == 0)
        {
            SchemaTypes = CreateDefaultCatalog();
        }

        SchemaTypeCatalogJson = JsonSerializer.Serialize(SchemaTypes);
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
            Message = Message,
            SafeReturnUrl = ResolveSafeReturnUrl()
        };
    }

    // Resolves a local return URL that is safe to use after popup completion.
    private string ResolveSafeReturnUrl()
    {
        string returnUrl = Request.Query[options.ReturnUrlQueryParameter];
        if (string.IsNullOrWhiteSpace(returnUrl))
        {
            return string.Empty;
        }

        if (!Url.IsLocalUrl(returnUrl))
        {
            return string.Empty;
        }

        return returnUrl;
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
            return "Editar tipo personalizado";
        }

        return "Nuevo tipo personalizado";
    }

    // Creates default system types.
    private static IReadOnlyCollection<SchemaTypeCatalogItem> CreateDefaultCatalog()
    {
        return
        [
            CreateCatalogItem("string"),
            CreateCatalogItem("number"),
            CreateCatalogItem("integer"),
            CreateCatalogItem("boolean"),
            CreateCatalogItem("obj" + "ect"),
            CreateCatalogItem("array")
        ];
    }

    // Creates a system catalog item.
    private static SchemaTypeCatalogItem CreateCatalogItem(string baseType)
    {
        return new SchemaTypeCatalogItem
        {
            Name = baseType,
            BaseType = baseType,
            VersionNumber = "1.0.0",
            IsSystem = true
        };
    }

}
