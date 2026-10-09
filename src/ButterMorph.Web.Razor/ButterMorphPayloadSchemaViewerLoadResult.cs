namespace ButterMorph.Web.Razor;

using ButterMorph.SchemaDesign;

/// <summary>
/// Describes host-provided schema viewer content.
/// </summary>
public sealed class ButterMorphPayloadSchemaViewerLoadResult
{
    /// <summary>
    /// Gets or sets the clean payload schema definition.
    /// </summary>
    public PayloadSchemaDefinition Definition { get; set; }

    /// <summary>
    /// Gets or sets raw schema JSON used when a definition is not available.
    /// </summary>
    public string JsonSchema { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a user-facing message.
    /// </summary>
    public string Message { get; set; } = string.Empty;
}
