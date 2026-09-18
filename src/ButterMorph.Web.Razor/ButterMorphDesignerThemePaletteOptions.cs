namespace ButterMorph.Web.Razor;

/// <summary>
/// Configures one visual color palette used by ButterMorph Razor designer pages.
/// </summary>
public sealed class ButterMorphDesignerThemePaletteOptions
{
    /// <summary>
    /// Gets or sets the main brand color.
    /// </summary>
    public string PrimaryColor { get; set; } = "#4f46e5";

    /// <summary>
    /// Gets or sets the hover color for primary actions.
    /// </summary>
    public string PrimaryHoverColor { get; set; } = "#4338ca";

    /// <summary>
    /// Gets or sets the darker brand color used by headings and accents.
    /// </summary>
    public string PrimaryDarkColor { get; set; } = "#3730a3";

    /// <summary>
    /// Gets or sets the page background color.
    /// </summary>
    public string BackgroundColor { get; set; } = "#f4f5fb";

    /// <summary>
    /// Gets or sets the main surface color.
    /// </summary>
    public string SurfaceColor { get; set; } = "#ffffff";

    /// <summary>
    /// Gets or sets the soft surface color used by nested panels.
    /// </summary>
    public string SurfaceSoftColor { get; set; } = "#fbfbff";

    /// <summary>
    /// Gets or sets the main text color.
    /// </summary>
    public string TextColor { get; set; } = "#1d2038";

    /// <summary>
    /// Gets or sets the muted text color.
    /// </summary>
    public string MutedTextColor { get; set; } = "#686d91";

    /// <summary>
    /// Gets or sets the standard border color.
    /// </summary>
    public string BorderColor { get; set; } = "#e2e3ef";

    /// <summary>
    /// Gets or sets the stronger border color.
    /// </summary>
    public string StrongBorderColor { get; set; } = "#d4d7eb";

    /// <summary>
    /// Gets or sets the danger color.
    /// </summary>
    public string DangerColor { get; set; } = "#b91c1c";

    /// <summary>
    /// Gets or sets the mapping designer sidebar background color.
    /// </summary>
    public string SidebarBackgroundColor { get; set; } = "#1a1a2e";

    /// <summary>
    /// Gets or sets the mapping designer sidebar brand background color.
    /// </summary>
    public string SidebarBrandBackgroundColor { get; set; } = "#141428";

    /// <summary>
    /// Gets or sets the mapping designer sidebar border color.
    /// </summary>
    public string SidebarBorderColor { get; set; } = "#2c2c50";

    /// <summary>
    /// Gets or sets the mapping designer sidebar text color.
    /// </summary>
    public string SidebarTextColor { get; set; } = "#d0d3e8";

    /// <summary>
    /// Gets or sets the mapping designer sidebar muted text color.
    /// </summary>
    public string SidebarMutedTextColor { get; set; } = "#9094b3";

    /// <summary>
    /// Gets or sets the mapping designer sidebar active text color.
    /// </summary>
    public string SidebarActiveTextColor { get; set; } = "#a5b4fc";

    internal static ButterMorphDesignerThemePaletteOptions CreateLight()
    {
        return new ButterMorphDesignerThemePaletteOptions();
    }

    internal static ButterMorphDesignerThemePaletteOptions CreateDark()
    {
        return new ButterMorphDesignerThemePaletteOptions
        {
            PrimaryColor = "#818cf8",
            PrimaryHoverColor = "#6366f1",
            PrimaryDarkColor = "#a5b4fc",
            BackgroundColor = "#0f172a",
            SurfaceColor = "#111827",
            SurfaceSoftColor = "#1f2937",
            TextColor = "#f8fafc",
            MutedTextColor = "#cbd5e1",
            BorderColor = "#334155",
            StrongBorderColor = "#475569",
            DangerColor = "#f87171",
            SidebarBackgroundColor = "#020617",
            SidebarBrandBackgroundColor = "#020617",
            SidebarBorderColor = "#1e293b",
            SidebarTextColor = "#e2e8f0",
            SidebarMutedTextColor = "#94a3b8",
            SidebarActiveTextColor = "#c7d2fe"
        };
    }
}
