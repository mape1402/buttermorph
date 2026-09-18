namespace ButterMorph.Web.Razor;

/// <summary>
/// Configures visual theme palettes used by ButterMorph Razor designer pages.
/// </summary>
public sealed class ButterMorphDesignerThemeOptions
{
    /// <summary>
    /// Gets or sets the initial theme mode used when the designer first renders.
    /// </summary>
    public ButterMorphDesignerThemeMode DefaultMode { get; set; } = ButterMorphDesignerThemeMode.Light;

    /// <summary>
    /// Gets or sets the initial theme mode used when the designer first renders.
    /// </summary>
    public ButterMorphDesignerThemeMode Mode
    {
        get => DefaultMode;
        set => DefaultMode = value;
    }

    /// <summary>
    /// Gets the light mode palette.
    /// </summary>
    public ButterMorphDesignerThemePaletteOptions Light { get; } = ButterMorphDesignerThemePaletteOptions.CreateLight();

    /// <summary>
    /// Gets the dark mode palette.
    /// </summary>
    public ButterMorphDesignerThemePaletteOptions Dark { get; } = ButterMorphDesignerThemePaletteOptions.CreateDark();

    /// <summary>
    /// Gets or sets the main brand color for the default palette.
    /// </summary>
    public string PrimaryColor
    {
        get => DefaultPalette.PrimaryColor;
        set => DefaultPalette.PrimaryColor = value;
    }

    /// <summary>
    /// Gets or sets the hover color for primary actions for the default palette.
    /// </summary>
    public string PrimaryHoverColor
    {
        get => DefaultPalette.PrimaryHoverColor;
        set => DefaultPalette.PrimaryHoverColor = value;
    }

    /// <summary>
    /// Gets or sets the darker brand color for the default palette.
    /// </summary>
    public string PrimaryDarkColor
    {
        get => DefaultPalette.PrimaryDarkColor;
        set => DefaultPalette.PrimaryDarkColor = value;
    }

    /// <summary>
    /// Gets or sets the page background color for the default palette.
    /// </summary>
    public string BackgroundColor
    {
        get => DefaultPalette.BackgroundColor;
        set => DefaultPalette.BackgroundColor = value;
    }

    /// <summary>
    /// Gets or sets the main surface color for the default palette.
    /// </summary>
    public string SurfaceColor
    {
        get => DefaultPalette.SurfaceColor;
        set => DefaultPalette.SurfaceColor = value;
    }

    /// <summary>
    /// Gets or sets the soft surface color for the default palette.
    /// </summary>
    public string SurfaceSoftColor
    {
        get => DefaultPalette.SurfaceSoftColor;
        set => DefaultPalette.SurfaceSoftColor = value;
    }

    /// <summary>
    /// Gets or sets the main text color for the default palette.
    /// </summary>
    public string TextColor
    {
        get => DefaultPalette.TextColor;
        set => DefaultPalette.TextColor = value;
    }

    /// <summary>
    /// Gets or sets the muted text color for the default palette.
    /// </summary>
    public string MutedTextColor
    {
        get => DefaultPalette.MutedTextColor;
        set => DefaultPalette.MutedTextColor = value;
    }

    /// <summary>
    /// Gets or sets the standard border color for the default palette.
    /// </summary>
    public string BorderColor
    {
        get => DefaultPalette.BorderColor;
        set => DefaultPalette.BorderColor = value;
    }

    /// <summary>
    /// Gets or sets the stronger border color for the default palette.
    /// </summary>
    public string StrongBorderColor
    {
        get => DefaultPalette.StrongBorderColor;
        set => DefaultPalette.StrongBorderColor = value;
    }

    /// <summary>
    /// Gets or sets the danger color for the default palette.
    /// </summary>
    public string DangerColor
    {
        get => DefaultPalette.DangerColor;
        set => DefaultPalette.DangerColor = value;
    }

    /// <summary>
    /// Gets or sets the mapping designer sidebar background color for the default palette.
    /// </summary>
    public string SidebarBackgroundColor
    {
        get => DefaultPalette.SidebarBackgroundColor;
        set => DefaultPalette.SidebarBackgroundColor = value;
    }

    /// <summary>
    /// Gets or sets the mapping designer sidebar brand background color for the default palette.
    /// </summary>
    public string SidebarBrandBackgroundColor
    {
        get => DefaultPalette.SidebarBrandBackgroundColor;
        set => DefaultPalette.SidebarBrandBackgroundColor = value;
    }

    /// <summary>
    /// Gets or sets the mapping designer sidebar border color for the default palette.
    /// </summary>
    public string SidebarBorderColor
    {
        get => DefaultPalette.SidebarBorderColor;
        set => DefaultPalette.SidebarBorderColor = value;
    }

    /// <summary>
    /// Gets or sets the mapping designer sidebar text color for the default palette.
    /// </summary>
    public string SidebarTextColor
    {
        get => DefaultPalette.SidebarTextColor;
        set => DefaultPalette.SidebarTextColor = value;
    }

    /// <summary>
    /// Gets or sets the mapping designer sidebar muted text color for the default palette.
    /// </summary>
    public string SidebarMutedTextColor
    {
        get => DefaultPalette.SidebarMutedTextColor;
        set => DefaultPalette.SidebarMutedTextColor = value;
    }

    /// <summary>
    /// Gets or sets the mapping designer sidebar active text color for the default palette.
    /// </summary>
    public string SidebarActiveTextColor
    {
        get => DefaultPalette.SidebarActiveTextColor;
        set => DefaultPalette.SidebarActiveTextColor = value;
    }

    /// <summary>
    /// Sets light mode as the initial designer mode.
    /// </summary>
    public void UseLightMode()
    {
        DefaultMode = ButterMorphDesignerThemeMode.Light;
    }

    /// <summary>
    /// Sets dark mode as the initial designer mode.
    /// </summary>
    public void UseDarkMode()
    {
        DefaultMode = ButterMorphDesignerThemeMode.Dark;
    }

    internal ButterMorphDesignerThemePaletteOptions GetPalette(ButterMorphDesignerThemeMode mode)
    {
        return mode == ButterMorphDesignerThemeMode.Dark ? Dark : Light;
    }

    private ButterMorphDesignerThemePaletteOptions DefaultPalette => GetPalette(DefaultMode);
}
