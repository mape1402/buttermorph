namespace ButterMorph.Web.Razor;

/// <summary>
/// Configures the visual colors used by ButterMorph Razor designer pages.
/// </summary>
public sealed class ButterMorphDesignerThemeOptions
{
    private string primaryColor;
    private string primaryHoverColor;
    private string primaryDarkColor;
    private string backgroundColor;
    private string surfaceColor;
    private string surfaceSoftColor;
    private string textColor;
    private string mutedTextColor;
    private string borderColor;
    private string strongBorderColor;
    private string dangerColor;
    private string sidebarBackgroundColor;
    private string sidebarBrandBackgroundColor;
    private string sidebarBorderColor;
    private string sidebarTextColor;
    private string sidebarMutedTextColor;
    private string sidebarActiveTextColor;

    /// <summary>
    /// Gets or sets the built-in theme mode.
    /// </summary>
    public ButterMorphDesignerThemeMode Mode { get; set; } = ButterMorphDesignerThemeMode.Light;

    /// <summary>
    /// Gets or sets the main brand color.
    /// </summary>
    public string PrimaryColor
    {
        get => primaryColor ?? Preset.PrimaryColor;
        set => primaryColor = value;
    }

    /// <summary>
    /// Gets or sets the hover color for primary actions.
    /// </summary>
    public string PrimaryHoverColor
    {
        get => primaryHoverColor ?? Preset.PrimaryHoverColor;
        set => primaryHoverColor = value;
    }

    /// <summary>
    /// Gets or sets the darker brand color used by headings and accents.
    /// </summary>
    public string PrimaryDarkColor
    {
        get => primaryDarkColor ?? Preset.PrimaryDarkColor;
        set => primaryDarkColor = value;
    }

    /// <summary>
    /// Gets or sets the page background color.
    /// </summary>
    public string BackgroundColor
    {
        get => backgroundColor ?? Preset.BackgroundColor;
        set => backgroundColor = value;
    }

    /// <summary>
    /// Gets or sets the main surface color.
    /// </summary>
    public string SurfaceColor
    {
        get => surfaceColor ?? Preset.SurfaceColor;
        set => surfaceColor = value;
    }

    /// <summary>
    /// Gets or sets the soft surface color used by nested panels.
    /// </summary>
    public string SurfaceSoftColor
    {
        get => surfaceSoftColor ?? Preset.SurfaceSoftColor;
        set => surfaceSoftColor = value;
    }

    /// <summary>
    /// Gets or sets the main text color.
    /// </summary>
    public string TextColor
    {
        get => textColor ?? Preset.TextColor;
        set => textColor = value;
    }

    /// <summary>
    /// Gets or sets the muted text color.
    /// </summary>
    public string MutedTextColor
    {
        get => mutedTextColor ?? Preset.MutedTextColor;
        set => mutedTextColor = value;
    }

    /// <summary>
    /// Gets or sets the standard border color.
    /// </summary>
    public string BorderColor
    {
        get => borderColor ?? Preset.BorderColor;
        set => borderColor = value;
    }

    /// <summary>
    /// Gets or sets the stronger border color.
    /// </summary>
    public string StrongBorderColor
    {
        get => strongBorderColor ?? Preset.StrongBorderColor;
        set => strongBorderColor = value;
    }

    /// <summary>
    /// Gets or sets the danger color.
    /// </summary>
    public string DangerColor
    {
        get => dangerColor ?? Preset.DangerColor;
        set => dangerColor = value;
    }

    /// <summary>
    /// Gets or sets the mapping designer sidebar background color.
    /// </summary>
    public string SidebarBackgroundColor
    {
        get => sidebarBackgroundColor ?? Preset.SidebarBackgroundColor;
        set => sidebarBackgroundColor = value;
    }

    /// <summary>
    /// Gets or sets the mapping designer sidebar brand background color.
    /// </summary>
    public string SidebarBrandBackgroundColor
    {
        get => sidebarBrandBackgroundColor ?? Preset.SidebarBrandBackgroundColor;
        set => sidebarBrandBackgroundColor = value;
    }

    /// <summary>
    /// Gets or sets the mapping designer sidebar border color.
    /// </summary>
    public string SidebarBorderColor
    {
        get => sidebarBorderColor ?? Preset.SidebarBorderColor;
        set => sidebarBorderColor = value;
    }

    /// <summary>
    /// Gets or sets the mapping designer sidebar text color.
    /// </summary>
    public string SidebarTextColor
    {
        get => sidebarTextColor ?? Preset.SidebarTextColor;
        set => sidebarTextColor = value;
    }

    /// <summary>
    /// Gets or sets the mapping designer sidebar muted text color.
    /// </summary>
    public string SidebarMutedTextColor
    {
        get => sidebarMutedTextColor ?? Preset.SidebarMutedTextColor;
        set => sidebarMutedTextColor = value;
    }

    /// <summary>
    /// Gets or sets the mapping designer sidebar active text color.
    /// </summary>
    public string SidebarActiveTextColor
    {
        get => sidebarActiveTextColor ?? Preset.SidebarActiveTextColor;
        set => sidebarActiveTextColor = value;
    }

    /// <summary>
    /// Switches to the built-in light theme and clears color overrides.
    /// </summary>
    public void UseLightMode()
    {
        Mode = ButterMorphDesignerThemeMode.Light;
        ClearColorOverrides();
    }

    /// <summary>
    /// Switches to the built-in dark theme and clears color overrides.
    /// </summary>
    public void UseDarkMode()
    {
        Mode = ButterMorphDesignerThemeMode.Dark;
        ClearColorOverrides();
    }

    /// <summary>
    /// Clears custom colors so the active mode preset is used.
    /// </summary>
    public void ClearColorOverrides()
    {
        primaryColor = null;
        primaryHoverColor = null;
        primaryDarkColor = null;
        backgroundColor = null;
        surfaceColor = null;
        surfaceSoftColor = null;
        textColor = null;
        mutedTextColor = null;
        borderColor = null;
        strongBorderColor = null;
        dangerColor = null;
        sidebarBackgroundColor = null;
        sidebarBrandBackgroundColor = null;
        sidebarBorderColor = null;
        sidebarTextColor = null;
        sidebarMutedTextColor = null;
        sidebarActiveTextColor = null;
    }

    private ButterMorphDesignerThemePreset Preset => Mode == ButterMorphDesignerThemeMode.Dark
        ? ButterMorphDesignerThemePreset.Dark
        : ButterMorphDesignerThemePreset.Light;

    private sealed class ButterMorphDesignerThemePreset
    {
        internal static readonly ButterMorphDesignerThemePreset Light = new()
        {
            PrimaryColor = "#4f46e5",
            PrimaryHoverColor = "#4338ca",
            PrimaryDarkColor = "#3730a3",
            BackgroundColor = "#f4f5fb",
            SurfaceColor = "#ffffff",
            SurfaceSoftColor = "#fbfbff",
            TextColor = "#1d2038",
            MutedTextColor = "#686d91",
            BorderColor = "#e2e3ef",
            StrongBorderColor = "#d4d7eb",
            DangerColor = "#b91c1c",
            SidebarBackgroundColor = "#1a1a2e",
            SidebarBrandBackgroundColor = "#141428",
            SidebarBorderColor = "#2c2c50",
            SidebarTextColor = "#d0d3e8",
            SidebarMutedTextColor = "#9094b3",
            SidebarActiveTextColor = "#a5b4fc"
        };

        internal static readonly ButterMorphDesignerThemePreset Dark = new()
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

        internal string PrimaryColor { get; init; }

        internal string PrimaryHoverColor { get; init; }

        internal string PrimaryDarkColor { get; init; }

        internal string BackgroundColor { get; init; }

        internal string SurfaceColor { get; init; }

        internal string SurfaceSoftColor { get; init; }

        internal string TextColor { get; init; }

        internal string MutedTextColor { get; init; }

        internal string BorderColor { get; init; }

        internal string StrongBorderColor { get; init; }

        internal string DangerColor { get; init; }

        internal string SidebarBackgroundColor { get; init; }

        internal string SidebarBrandBackgroundColor { get; init; }

        internal string SidebarBorderColor { get; init; }

        internal string SidebarTextColor { get; init; }

        internal string SidebarMutedTextColor { get; init; }

        internal string SidebarActiveTextColor { get; init; }
    }
}
