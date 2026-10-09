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
        Append(builder, "--bm-primary-dark", palette.PrimaryDarkColor);
        Append(builder, "--bm-danger", palette.DangerColor);
        Append(builder, "--bm-on-primary", "#ffffff");
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
        AppendDerivedThemeVariables(builder);
    }

    private static void AppendDerivedThemeVariables(StringBuilder builder)
    {
        Append(builder, "--bm-accent-surface", "color-mix(in srgb, var(--bm-primary) 8%, var(--bm-surface))");
        Append(builder, "--bm-accent-soft", "color-mix(in srgb, var(--bm-primary) 10%, var(--bm-content-bg))");
        Append(builder, "--bm-accent-border", "color-mix(in srgb, var(--bm-primary) 28%, var(--bm-border))");
        Append(builder, "--bm-accent-strong-border", "color-mix(in srgb, var(--bm-primary) 48%, var(--bm-border-strong))");
        Append(builder, "--bm-accent-text", "color-mix(in srgb, var(--bm-primary) 78%, var(--bm-text))");
        Append(builder, "--bm-accent-text-strong", "color-mix(in srgb, var(--bm-primary) 86%, var(--bm-text))");
        Append(builder, "--bm-focus-ring", "color-mix(in srgb, var(--bm-primary) 24%, transparent)");
        Append(builder, "--bm-drop-surface", "color-mix(in srgb, var(--bm-primary) 10%, var(--bm-surface))");
        Append(builder, "--bm-chip-bg", "color-mix(in srgb, var(--bm-primary) 9%, var(--bm-surface))");
        Append(builder, "--bm-chip-border", "color-mix(in srgb, var(--bm-primary) 30%, var(--bm-border))");
        Append(builder, "--bm-chip-text", "color-mix(in srgb, var(--bm-primary) 82%, var(--bm-text))");
        Append(builder, "--bm-sidebar-hover", "color-mix(in srgb, var(--bm-sidebar-accent) 10%, transparent)");
        Append(builder, "--bm-sidebar-active", "color-mix(in srgb, var(--bm-sidebar-accent) 18%, transparent)");
        Append(builder, "--bm-sidebar-accent-surface", "color-mix(in srgb, var(--bm-sidebar-accent) 14%, var(--bm-sidebar-bg))");
        Append(builder, "--bm-sidebar-accent-surface-strong", "color-mix(in srgb, var(--bm-sidebar-accent) 24%, var(--bm-sidebar-bg))");
        Append(builder, "--bm-sidebar-accent-border", "color-mix(in srgb, var(--bm-sidebar-accent) 30%, var(--bm-sidebar-border))");
        Append(builder, "--bm-sidebar-accent-strong-border", "color-mix(in srgb, var(--bm-sidebar-accent) 46%, var(--bm-sidebar-border))");
        Append(builder, "--bm-danger-soft", "color-mix(in srgb, var(--bm-danger) 8%, var(--bm-surface))");
        Append(builder, "--bm-danger-border", "color-mix(in srgb, var(--bm-danger) 30%, var(--bm-border))");
        Append(builder, "--bm-danger-text", "color-mix(in srgb, var(--bm-danger) 82%, var(--bm-text))");
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
