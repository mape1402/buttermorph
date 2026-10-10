namespace ButterMorph.Web.Razor;

using Microsoft.AspNetCore.Mvc.RazorPages;

internal static class ButterMorphDesignerThemePageExtensions
{
    internal static string BuildThemeStyle(this PageModel pageModel, ButterMorphRazorDesignerOptions options)
    {
        return ButterMorphDesignerThemeStyle.Build(options, pageModel.ResolveThemeMode(options));
    }

    internal static string ResolveThemeModeName(this PageModel pageModel, ButterMorphRazorDesignerOptions options)
    {
        return pageModel.ResolveThemeMode(options).ToString().ToLowerInvariant();
    }

    private static ButterMorphDesignerThemeMode ResolveThemeMode(this PageModel pageModel, ButterMorphRazorDesignerOptions options)
    {
        string requestedMode = pageModel.Request.Query["themeMode"].ToString();
        if (string.IsNullOrWhiteSpace(requestedMode))
        {
            requestedMode = pageModel.Request.Query["theme"].ToString();
        }

        if (string.Equals(requestedMode, "light", StringComparison.OrdinalIgnoreCase))
        {
            return ButterMorphDesignerThemeMode.Light;
        }

        if (string.Equals(requestedMode, "dark", StringComparison.OrdinalIgnoreCase))
        {
            return ButterMorphDesignerThemeMode.Dark;
        }

        return options.Theme.DefaultMode;
    }
}
