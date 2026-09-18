namespace ButterMorph.Web.Razor;

using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Options;

/// <summary>
/// Displays the ButterMorph designer dashboard.
/// </summary>
public sealed class IndexModel : PageModel
{
    // Reads designer integration options.
    private readonly ButterMorphRazorDesignerOptions options;

    /// <summary>
    /// Initializes a new instance of the <see cref="IndexModel"/> class.
    /// </summary>
    /// <param name="options">The Razor designer options.</param>
    public IndexModel(IOptions<ButterMorphRazorDesignerOptions> options)
    {
        this.options = options.Value;
    }

    /// <summary>
    /// Gets the host-configured designer theme style.
    /// </summary>
    public string ThemeStyle => ButterMorphDesignerThemeStyle.Build(options);

    /// <summary>
    /// Gets the host-configured designer theme mode.
    /// </summary>
    public string ThemeMode => options.Theme.Mode.ToString().ToLowerInvariant();
}
