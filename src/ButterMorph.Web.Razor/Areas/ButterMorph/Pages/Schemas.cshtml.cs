namespace ButterMorph.Web.Razor;

using ButterMorph.Design;
using ButterMorph.Json.Schema;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Options;

/// <summary>
/// Loads source and target schemas into the designer session.
/// </summary>
public sealed class SchemasModel : PageModel
{
    // Stores design sessions for the web designer.
    private readonly IMappingDesignSessionStore _sessionStore;

    // Imports pasted JSON Schema text.
    private readonly IJsonSchemaImporter _schemaImporter;

    // Reads designer integration options.
    private readonly ButterMorphRazorDesignerOptions options;

    /// <summary>
    /// Initializes a new instance of the <see cref="SchemasModel"/> class.
    /// </summary>
    /// <param name="sessionStore">The session store.</param>
    /// <param name="schemaImporter">The JSON Schema importer.</param>
    /// <param name="options">The Razor designer options.</param>
    public SchemasModel(
        IMappingDesignSessionStore sessionStore,
        IJsonSchemaImporter schemaImporter,
        IOptions<ButterMorphRazorDesignerOptions> options)
    {
        _sessionStore = sessionStore;
        _schemaImporter = schemaImporter;
        this.options = options.Value;
    }

    /// <summary>
    /// Gets the host-configured designer theme style.
    /// </summary>
    public string ThemeStyle => ButterMorphDesignerThemeStyle.Build(options);

    /// <summary>
    /// Gets the host-configured designer theme mode.
    /// </summary>
    public string ThemeMode => options.Theme.DefaultMode.ToString().ToLowerInvariant();

    /// <summary>
    /// Gets or sets the source key.
    /// </summary>
    [BindProperty]
    public string SourceKey { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the source JSON Schema text.
    /// </summary>
    [BindProperty]
    public string SourceSchemaJson { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the target JSON Schema text.
    /// </summary>
    [BindProperty]
    public string TargetSchemaJson { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the status message.
    /// </summary>
    public string Message { get; set; } = "Paste JSON Schemas and load them into the design session.";

    /// <summary>
    /// Loads the source schema.
    /// </summary>
    /// <returns>The page result.</returns>
    public IActionResult OnPostSource()
    {
        JsonSchemaConversionResult result = _schemaImporter.Import(new JsonSchemaImportRequest
        {
            Name = SourceKey,
            JsonSchema = SourceSchemaJson
        });

        if (result.Succeeded)
        {
            _sessionStore.GetOrCreate(DesignerSessionKeyResolver.Resolve(this)).LoadSourceSchema(SourceKey, result.Schema);
            Message = "Source schema loaded.";
        }
        else
        {
            Message = "Source schema could not be loaded.";
        }

        return Page();
    }

    /// <summary>
    /// Loads the target schema.
    /// </summary>
    /// <returns>The page result.</returns>
    public IActionResult OnPostTarget()
    {
        JsonSchemaConversionResult result = _schemaImporter.Import(new JsonSchemaImportRequest
        {
            Name = "Target",
            JsonSchema = TargetSchemaJson
        });

        if (result.Succeeded)
        {
            _sessionStore.GetOrCreate(DesignerSessionKeyResolver.Resolve(this)).LoadTargetSchema(result.Schema);
            Message = "Target schema loaded.";
        }
        else
        {
            Message = "Target schema could not be loaded.";
        }

        return Page();
    }
}
