namespace ButterMorph.Web.Razor;

using ButterMorph.SchemaDesign;

/// <summary>
/// Represents a field metadata designer save request.
/// </summary>
public sealed class ButterMorphFieldMetadataDesignerSaveRequest
{
    /// <summary>
    /// Gets or sets the host context key.
    /// </summary>
    public string ContextKey { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the field metadata design result.
    /// </summary>
    public FieldMetadataDesignResult Result { get; set; } = new();
}
