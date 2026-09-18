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
        builder.Append("color-scheme:");
        builder.Append(theme.Mode == ButterMorphDesignerThemeMode.Dark ? "dark" : "light");
        builder.Append(';');

        Append(builder, "--bm-content-bg", theme.BackgroundColor);
        Append(builder, "--bm-surface", theme.SurfaceColor);
        Append(builder, "--bm-border", theme.BorderColor);
        Append(builder, "--bm-border-strong", theme.StrongBorderColor);
        Append(builder, "--bm-text", theme.TextColor);
        Append(builder, "--bm-muted", theme.MutedTextColor);
        Append(builder, "--bm-primary", theme.PrimaryColor);
        Append(builder, "--bm-primary-hover", theme.PrimaryHoverColor);
        Append(builder, "--bm-danger", theme.DangerColor);
        Append(builder, "--bm-sidebar-bg", theme.SidebarBackgroundColor);
        Append(builder, "--bm-sidebar-brand-bg", theme.SidebarBrandBackgroundColor);
        Append(builder, "--bm-sidebar-border", theme.SidebarBorderColor);
        Append(builder, "--bm-sidebar-text", theme.SidebarTextColor);
        Append(builder, "--bm-sidebar-muted", theme.SidebarMutedTextColor);
        Append(builder, "--bm-sidebar-accent", theme.PrimaryColor);
        Append(builder, "--bm-sidebar-active-text", theme.SidebarActiveTextColor);

        Append(builder, "--bm-schema-bg", theme.BackgroundColor);
        Append(builder, "--bm-schema-panel", theme.SurfaceColor);
        Append(builder, "--bm-schema-panel-soft", theme.SurfaceSoftColor);
        Append(builder, "--bm-schema-border", theme.BorderColor);
        Append(builder, "--bm-schema-border-strong", theme.StrongBorderColor);
        Append(builder, "--bm-schema-text", theme.TextColor);
        Append(builder, "--bm-schema-muted", theme.MutedTextColor);
        Append(builder, "--bm-schema-primary", theme.PrimaryColor);
        Append(builder, "--bm-schema-primary-dark", theme.PrimaryDarkColor);
        Append(builder, "--bm-schema-danger", theme.DangerColor);

        return builder.ToString();
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
