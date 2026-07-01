namespace ButterMorph.Web.Razor;

using ButterMorph.SchemaDesign;

/// <summary>
/// Represents field metadata designer load state.
/// </summary>
public sealed class ButterMorphFieldMetadataDesignerLoadResult
{
    /// <summary>
    /// Gets or sets the editable metadata input.
    /// </summary>
    public FieldMetadataDesignInput Input { get; set; } = new();

    /// <summary>
    /// Gets or sets the saved custom field definition.
    /// </summary>
    public CustomFieldDefinition Definition { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether manual actions are shown.
    /// </summary>
    public bool ShowManualActions { get; set; } = true;

    /// <summary>
    /// Gets or sets an optional user-facing message.
    /// </summary>
    public string Message { get; set; } = string.Empty;
}
