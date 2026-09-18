namespace ButterMorph.Web.Razor;

using System.Text;

// Builds inline CSS variables for host-configured designer themes.
internal static class ButterMorphDesignerThemeStyle
{
    // Prevents breaking out of the inline style attribute.
    private static readonly char[] BlockedCharacters = [';', '{', '}', '<', '>', '"', '\''];

    internal static string Build(ButterMorphRazorDesignerOptions options)
    {
        return Build(options.Theme);
    }

    internal static string Build(ButterMorphDesignerThemeOptions theme)
    {
        StringBuilder builder = new();
        ButterMorphDesignerThemePaletteOptions activePalette = theme.GetPalette(theme.DefaultMode);

        builder.Append("color-scheme:");
        builder.Append(theme.DefaultMode == ButterMorphDesignerThemeMode.Dark ? "dark" : "light");
        builder.Append(';');

        AppendPalette(builder, "light", theme.Light);
        AppendPalette(builder, "dark", theme.Dark);
        AppendActivePalette(builder, activePalette);

        return builder.ToString();
    }

    private static void AppendPalette(StringBuilder builder, string mode, ButterMorphDesignerThemePaletteOptions palette)
    {
        Append(builder, "--bm-theme-" + mode + "-primary-color", palette.PrimaryColor);
        Append(builder, "--bm-theme-" + mode + "-primary-hover-color", palette.PrimaryHoverColor);
        Append(builder, "--bm-theme-" + mode + "-primary-dark-color", palette.PrimaryDarkColor);
        Append(builder, "--bm-theme-" + mode + "-background-color", palette.BackgroundColor);
        Append(builder, "--bm-theme-" + mode + "-surface-color", palette.SurfaceColor);
        Append(builder, "--bm-theme-" + mode + "-surface-soft-color", palette.SurfaceSoftColor);
        Append(builder, "--bm-theme-" + mode + "-text-color", palette.TextColor);
        Append(builder, "--bm-theme-" + mode + "-muted-text-color", palette.MutedTextColor);
        Append(builder, "--bm-theme-" + mode + "-border-color", palette.BorderColor);
        Append(builder, "--bm-theme-" + mode + "-strong-border-color", palette.StrongBorderColor);
        Append(builder, "--bm-theme-" + mode + "-danger-color", palette.DangerColor);
        Append(builder, "--bm-theme-" + mode + "-sidebar-background-color", palette.SidebarBackgroundColor);
        Append(builder, "--bm-theme-" + mode + "-sidebar-brand-background-color", palette.SidebarBrandBackgroundColor);
        Append(builder, "--bm-theme-" + mode + "-sidebar-border-color", palette.SidebarBorderColor);
        Append(builder, "--bm-theme-" + mode + "-sidebar-text-color", palette.SidebarTextColor);
        Append(builder, "--bm-theme-" + mode + "-sidebar-muted-text-color", palette.SidebarMutedTextColor);
        Append(builder, "--bm-theme-" + mode + "-sidebar-active-text-color", palette.SidebarActiveTextColor);
    }

    private static void AppendActivePalette(StringBuilder builder, ButterMorphDesignerThemePaletteOptions palette)
    {
        Append(builder, "--bm-content-bg", palette.BackgroundColor);
        Append(builder, "--bm-surface", palette.SurfaceColor);
        Append(builder, "--bm-border", palette.BorderColor);
        Append(builder, "--bm-border-strong", palette.StrongBorderColor);
        Append(builder, "--bm-text", palette.TextColor);
        Append(builder, "--bm-muted", palette.MutedTextColor);
        Append(builder, "--bm-primary", palette.PrimaryColor);
        Append(builder, "--bm-primary-hover", palette.PrimaryHoverColor);
        Append(builder, "--bm-danger", palette.DangerColor);
        Append(builder, "--bm-sidebar-bg", palette.SidebarBackgroundColor);
        Append(builder, "--bm-sidebar-brand-bg", palette.SidebarBrandBackgroundColor);
        Append(builder, "--bm-sidebar-border", palette.SidebarBorderColor);
        Append(builder, "--bm-sidebar-text", palette.SidebarTextColor);
        Append(builder, "--bm-sidebar-muted", palette.SidebarMutedTextColor);
        Append(builder, "--bm-sidebar-accent", palette.PrimaryColor);
        Append(builder, "--bm-sidebar-active-text", palette.SidebarActiveTextColor);

        Append(builder, "--bm-schema-bg", palette.BackgroundColor);
        Append(builder, "--bm-schema-panel", palette.SurfaceColor);
        Append(builder, "--bm-schema-panel-soft", palette.SurfaceSoftColor);
        Append(builder, "--bm-schema-border", palette.BorderColor);
        Append(builder, "--bm-schema-border-strong", palette.StrongBorderColor);
        Append(builder, "--bm-schema-text", palette.TextColor);
        Append(builder, "--bm-schema-muted", palette.MutedTextColor);
        Append(builder, "--bm-schema-primary", palette.PrimaryColor);
        Append(builder, "--bm-schema-primary-dark", palette.PrimaryDarkColor);
        Append(builder, "--bm-schema-danger", palette.DangerColor);
    }

    private static void Append(StringBuilder builder, string name, string value)
    {
        string safeValue = Sanitize(value);
        if (string.IsNullOrWhiteSpace(safeValue))
        {
            return;
        }

        builder.Append(name);
        builder.Append(':');
        builder.Append(safeValue);
        builder.Append(';');
    }

    private static string Sanitize(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        if (value.IndexOfAny(BlockedCharacters) >= 0)
        {
            return string.Empty;
        }

        return value.Trim();
    }
}
