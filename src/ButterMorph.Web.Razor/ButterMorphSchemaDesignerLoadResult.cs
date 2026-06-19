namespace ButterMorph.Web.Razor;

using ButterMorph.Abstractions;

/// <summary>
/// Represents host-provided schema designer state.
/// </summary>
public sealed class ButterMorphSchemaDesignerLoadResult
{
    /// <summary>
    /// Gets or sets the schema to edit.
    /// </summary>
    public IStructureSchema Schema { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether manual import and export actions are shown.
    /// </summary>
    public bool ShowManualActions { get; set; } = true;

    /// <summary>
    /// Gets or sets the optional load message.
    /// </summary>
    public string Message { get; set; } = string.Empty;
}
