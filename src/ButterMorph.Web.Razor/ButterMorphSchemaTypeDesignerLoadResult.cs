namespace ButterMorph.Web.Razor;

using ButterMorph.SchemaDesign;

/// <summary>
/// Represents schema type designer load state.
/// </summary>
public sealed class ButterMorphSchemaTypeDesignerLoadResult
{
    /// <summary>
    /// Gets or sets the editable type input.
    /// </summary>
    public SchemaTypeDesignInput Input { get; set; } = new();

    /// <summary>
    /// Gets or sets the available schema type catalog.
    /// </summary>
    public IReadOnlyCollection<SchemaTypeCatalogItem> SchemaTypes { get; set; } = [];

    /// <summary>
    /// Gets or sets a value indicating whether manual import or catalog actions are shown.
    /// </summary>
    public bool ShowManualActions { get; set; } = true;

    /// <summary>
    /// Gets or sets an optional user-facing message.
    /// </summary>
    public string Message { get; set; } = string.Empty;
}
