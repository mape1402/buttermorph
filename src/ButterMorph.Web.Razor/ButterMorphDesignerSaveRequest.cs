namespace ButterMorph.Web.Razor;

using ButterMorph.Abstractions;

/// <summary>
/// Represents a request to save designer state into a host application.
/// </summary>
public sealed class ButterMorphDesignerSaveRequest
{
    /// <summary>
    /// Gets or sets the host context key.
    /// </summary>
    public string ContextKey { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the transformation document to save.
    /// </summary>
    public ITransformationDocument Document { get; set; }

    /// <summary>
    /// Gets or sets the exported DSL content.
    /// </summary>
    public string DslContent { get; set; } = string.Empty;
}
