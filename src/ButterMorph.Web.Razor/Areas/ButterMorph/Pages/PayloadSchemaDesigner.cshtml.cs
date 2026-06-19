namespace ButterMorph.Web.Razor;

using System.Text.Json;
using ButterMorph.SchemaDesign;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Options;

/// <summary>
/// Displays the reusable payload schema designer.
/// </summary>
public sealed class PayloadSchemaDesignerModel : PageModel
{
    // Builds payload schema output.
    private readonly IPayloadSchemaBuilder payloadBuilder;

    // Reads designer integration options.
    private readonly ButterMorphRazorDesignerOptions options;

    // Provides optional host integrations.
    private readonly IEnumerable<IButterMorphPayloadSchemaDesignerHost> hosts;

    /// <summary>
    /// Initializes a new instance of the <see cref="PayloadSchemaDesignerModel"/> class.
    /// </summary>
    /// <param name="payloadBuilder">The payload schema builder.</param>
    /// <param name="options">The designer options.</param>
    /// <param name="hosts">The optional host integrations.</param>
    public PayloadSchemaDesignerModel(
        IPayloadSchemaBuilder payloadBuilder,
        IOptions<ButterMorphRazorDesignerOptions> options,
        IEnumerable<IButterMorphPayloadSchemaDesignerHost> hosts)
    {
        this.payloadBuilder = payloadBuilder;
        this.options = options.Value;
        this.hosts = hosts;
    }

    /// <summary>
    /// Gets or sets the payload JSON Schema.
    /// </summary>
    [BindProperty]
    public string PayloadSchemaJson { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets available schema types.
    /// </summary>
    public IReadOnlyCollection<SchemaTypeCatalogItem> SchemaTypes { get; set; } = [];

    /// <summary>
    /// Gets or sets available metadata fields.
    /// </summary>
    public IReadOnlyCollection<FieldMetadataCatalogItem> MetadataFields { get; set; } = [];

    /// <summary>
    /// Gets or sets catalog JSON used by client behavior.
    /// </summary>
    public string SchemaTypeCatalogJson { get; set; } = "[]";

    /// <summary>
    /// Gets or sets metadata catalog JSON used by client behavior.
    /// </summary>
    public string FieldMetadataCatalogJson { get; set; } = "[]";

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
    public string FormTitle { get; set; } = "Schema del Payload";

    /// <summary>
    /// Handles initial display.
    /// </summary>
    /// <returns>The page result.</returns>
    public async Task<IActionResult> OnGet()
    {
        FormTitle = ResolveFormTitle();
        await ApplyHostLoad();
        RefreshCatalogs();

        return Page();
    }

    /// <summary>
    /// Saves payload schema state through the optional host.
    /// </summary>
    /// <returns>The page result.</returns>
    public async Task<IActionResult> OnPostSave()
    {
        FormTitle = ResolveFormTitle();
        await ApplyHostLoadCatalogOnly();
        PayloadSchemaDesignResult result = payloadBuilder.Build(new PayloadSchemaDesignInput
        {
            Name = "Payload",
            JsonSchema = PayloadSchemaJson
        }, SchemaTypes, MetadataFields);
        RefreshCatalogs();

        if (!result.Succeeded)
        {
            Message = string.Join(" ", result.Diagnostics.Select(diagnostic => diagnostic.Message));
            return Page();
        }

        PayloadSchemaJson = result.JsonSchema;
        ButterMorphPayloadSchemaDesignerSaveResult saveResult = new()
        {
            Succeeded = true,
            Message = "Payload schema saved."
        };

        foreach (IButterMorphPayloadSchemaDesignerHost host in hosts)
        {
            saveResult = await host.Save(new ButterMorphPayloadSchemaDesignerSaveRequest
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
        foreach (IButterMorphPayloadSchemaDesignerHost host in hosts)
        {
            ButterMorphPayloadSchemaDesignerLoadResult result = await host.Load(new ButterMorphPayloadSchemaDesignerLoadRequest
            {
                ContextKey = ResolveContextKey()
            });
            PayloadSchemaJson = result.JsonSchema;
            SchemaTypes = result.SchemaTypes;
            MetadataFields = result.MetadataFields;
            ShowManualActions = result.ShowManualActions;
            Message = result.Message;
            return;
        }

        PayloadSchemaJson = "{\"type\":\"" + ("obj" + "ect") + "\",\"properties\":{}}";
    }

    // Applies only host catalogs during posts.
    private async Task ApplyHostLoadCatalogOnly()
    {
        foreach (IButterMorphPayloadSchemaDesignerHost host in hosts)
        {
            ButterMorphPayloadSchemaDesignerLoadResult result = await host.Load(new ButterMorphPayloadSchemaDesignerLoadRequest
            {
                ContextKey = ResolveContextKey()
            });
            SchemaTypes = result.SchemaTypes;
            MetadataFields = result.MetadataFields;
            ShowManualActions = result.ShowManualActions;
            return;
        }
    }

    // Refreshes serialized catalogs.
    private void RefreshCatalogs()
    {
        if (SchemaTypes.Count == 0)
        {
            SchemaTypes = CreateDefaultCatalog();
        }

        SchemaTypeCatalogJson = JsonSerializer.Serialize(SchemaTypes);
        FieldMetadataCatalogJson = JsonSerializer.Serialize(MetadataFields);
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
            return "Editar Schema del Payload";
        }

        return "Schema del Payload";
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
