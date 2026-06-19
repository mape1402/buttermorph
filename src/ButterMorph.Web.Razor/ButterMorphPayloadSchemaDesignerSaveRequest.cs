namespace ButterMorph.Web.Razor;

using ButterMorph.SchemaDesign;

/// <summary>
/// Represents a payload schema designer save request.
/// </summary>
public sealed class ButterMorphPayloadSchemaDesignerSaveRequest
{
    /// <summary>
    /// Gets or sets the host context key.
    /// </summary>
    public string ContextKey { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the payload schema design result.
    /// </summary>
    public PayloadSchemaDesignResult Result { get; set; } = new();
}