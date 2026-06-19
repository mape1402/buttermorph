namespace ButterMorph.Web.Razor;

using ButterMorph.Abstractions;

/// <summary>
/// Represents a schema designer save request.
/// </summary>
public sealed class ButterMorphSchemaDesignerSaveRequest
{
    /// <summary>
    /// Gets or sets the host context key.
    /// </summary>
    public string ContextKey { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the schema being saved.
    /// </summary>
    public IStructureSchema Schema { get; set; }

    /// <summary>
    /// Gets or sets the exported JSON Schema text.
    /// </summary>
    public string JsonSchema { get; set; } = string.Empty;
}
