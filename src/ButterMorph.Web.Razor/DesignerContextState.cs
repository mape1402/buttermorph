namespace ButterMorph.Web.Razor;

// Stores web-only state for a mounted designer context.
internal sealed class DesignerContextState
{
    // Tracks whether host preload has already been applied.
    internal bool HostPreloadApplied { get; set; }

    // Controls whether manual schema action buttons are rendered.
    internal bool ShowSchemaActions { get; set; } = true;
}
