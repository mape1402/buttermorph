namespace ButterMorph.Web.Razor;

using ButterMorph.SchemaDesign;

/// <summary>
/// Represents a schema type designer save request.
/// </summary>
public sealed class ButterMorphSchemaTypeDesignerSaveRequest
{
    /// <summary>
    /// Gets or sets the host context key.
    /// </summary>
    public string ContextKey { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the schema type design result.
    /// </summary>
    public SchemaTypeDesignResult Result { get; set; } = new();
}