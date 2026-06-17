namespace ButterMorph.Web.Razor;

using ButterMorph.Design;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

/// <summary>
/// Imports and exports ButterMorph DSL content.
/// </summary>
public sealed class DslModel : PageModel
{
    // Stores design sessions for the web designer.
    private readonly IMappingDesignSessionStore _sessionStore;

    /// <summary>
    /// Initializes a new instance of the <see cref="DslModel"/> class.
    /// </summary>
    /// <param name="sessionStore">The session store.</param>
    public DslModel(IMappingDesignSessionStore sessionStore)
    {
        _sessionStore = sessionStore;
    }

    /// <summary>
    /// Gets or sets the DSL content.
    /// </summary>
    [BindProperty]
    public string DslContent { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the status message.
    /// </summary>
    public string Message { get; set; } = "Import or export the current mapping document.";

    /// <summary>
    /// Displays exported DSL content.
    /// </summary>
    public void OnGet()
    {
        DslContent = Session.ExportDsl();
    }

    /// <summary>
    /// Imports posted DSL content.
    /// </summary>
    /// <returns>The page result.</returns>
    public IActionResult OnPostImport()
    {
        IMappingOperationResult result = Session.ImportDsl(DslContent);
        Message = CreateMessage(result, "DSL imported.");

        return Page();
    }

    /// <summary>
    /// Exports current DSL content.
    /// </summary>
    /// <returns>The page result.</returns>
    public IActionResult OnPostExport()
    {
        DslContent = Session.ExportDsl();
        Message = "DSL exported.";

        return Page();
    }

    // Gets the current design session.
    private IMappingDesignSession Session => _sessionStore.GetOrCreate(DesignerSessionKeys.DefaultSessionKey);

    // Creates a friendly operation message.
    private static string CreateMessage(IMappingOperationResult result, string successMessage)
    {
        if (result.Succeeded)
        {
            return successMessage;
        }

        foreach (ButterMorph.Abstractions.DiagnosticEntry diagnostic in result.Diagnostics)
        {
            return diagnostic.Message;
        }

        return "Operation failed.";
    }
}
