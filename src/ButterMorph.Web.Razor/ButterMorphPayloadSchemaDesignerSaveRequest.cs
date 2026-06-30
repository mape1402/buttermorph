namespace ButterMorph.Web.Razor;

using ButterMorph.SchemaDesign;

/// <summary>
/// Represents a schema designer save request.
/// </summary>
public sealed class ButterMorphPayloadSchemaDesignerSaveRequest
{
    /// <summary>
    /// Gets or sets the host context key.
    /// </summary>
    public string ContextKey { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the clean payload schema definition.
    /// </summary>
    public PayloadSchemaDefinition Definition { get; set; } = new();

    /// <summary>
    /// Gets or sets host custom type ids used for this saved schema.
    /// </summary>
    public IReadOnlyCollection<string> InjectedCustomTypeIds { get; set; } = [];

    /// <summary>
    /// Gets or sets host custom field ids used for this saved schema.
    /// </summary>
    public IReadOnlyCollection<string> InjectedCustomFieldIds { get; set; } = [];
}
