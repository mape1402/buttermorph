namespace ButterMorph.Web.Razor;

using ButterMorph.Design;
using ButterMorph.Json.Schema;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

/// <summary>
/// Loads source and target schemas into the designer session.
/// </summary>
public sealed class SchemasModel : PageModel
{
    // Stores design sessions for the web designer.
    private readonly IMappingDesignSessionStore _sessionStore;

    // Imports pasted JSON Schema text.
    private readonly IJsonSchemaImporter _schemaImporter;

    /// <summary>
    /// Initializes a new instance of the <see cref="SchemasModel"/> class.
    /// </summary>
    /// <param name="sessionStore">The session store.</param>
    /// <param name="schemaImporter">The JSON Schema importer.</param>
    public SchemasModel(IMappingDesignSessionStore sessionStore, IJsonSchemaImporter schemaImporter)
    {
        _sessionStore = sessionStore;
        _schemaImporter = schemaImporter;
    }

    /// <summary>
    /// Gets or sets the source key.
    /// </summary>
    [BindProperty]
    public string SourceKey { get; set; } = "source";

    /// <summary>
    /// Gets or sets the source JSON Schema text.
    /// </summary>
    [BindProperty]
    public string SourceSchemaJson { get; set; } = SampleSchemas.Source;

    /// <summary>
    /// Gets or sets the target JSON Schema text.
    /// </summary>
    [BindProperty]
    public string TargetSchemaJson { get; set; } = SampleSchemas.Target;

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
            _sessionStore.GetOrCreate(DesignerSessionKeys.DefaultSessionKey).LoadSourceSchema(SourceKey, result.Schema);
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
            _sessionStore.GetOrCreate(DesignerSessionKeys.DefaultSessionKey).LoadTargetSchema(result.Schema);
            Message = "Target schema loaded.";
        }
        else
        {
            Message = "Target schema could not be loaded.";
        }

        return Page();
    }

    /// <summary>
    /// Loads demo source and target schemas.
    /// </summary>
    /// <returns>The page result.</returns>
    public IActionResult OnPostDemo()
    {
        JsonSchemaConversionResult sourceResult = _schemaImporter.Import(new JsonSchemaImportRequest
        {
            Name = SourceKey,
            JsonSchema = SourceSchemaJson
        });
        JsonSchemaConversionResult targetResult = _schemaImporter.Import(new JsonSchemaImportRequest
        {
            Name = "Target",
            JsonSchema = TargetSchemaJson
        });

        if (sourceResult.Succeeded && targetResult.Succeeded)
        {
            IMappingDesignSession session = _sessionStore.GetOrCreate(DesignerSessionKeys.DefaultSessionKey);
            session.LoadSourceSchema(SourceKey, sourceResult.Schema);
            session.LoadTargetSchema(targetResult.Schema);
            Message = "Demo schemas loaded. Open the designer and add a mapping.";
        }
        else
        {
            Message = "Demo schemas could not be loaded.";
        }

        return Page();
    }
}
