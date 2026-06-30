namespace ButterMorph.Web.Razor;

/// <summary>
/// Represents a schema designer load request.
/// </summary>
public sealed class ButterMorphPayloadSchemaDesignerLoadRequest
{
    /// <summary>
    /// Gets or sets the host context key.
    /// </summary>
    public string ContextKey { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets host custom type ids requested for this designer launch.
    /// </summary>
    public IReadOnlyCollection<string> InjectedCustomTypeIds { get; set; } = [];

    /// <summary>
    /// Gets or sets host custom field ids requested for this designer launch.
    /// </summary>
    public IReadOnlyCollection<string> InjectedCustomFieldIds { get; set; } = [];
}
