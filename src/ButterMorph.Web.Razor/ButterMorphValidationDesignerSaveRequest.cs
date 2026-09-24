namespace ButterMorph.Web.Razor;

using ButterMorph.Abstractions;

/// <summary>
/// Represents a request to save validation designer state into a host application.
/// </summary>
public sealed class ButterMorphValidationDesignerSaveRequest
{
    /// <summary>
    /// Gets or sets the host context key.
    /// </summary>
    public string ContextKey { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the validation document to save.
    /// </summary>
    public IValidationDocument Document { get; set; }

    /// <summary>
    /// Gets or sets source metadata keyed by source name.
    /// </summary>
    public IReadOnlyDictionary<string, ButterMorphDesignerSourceMetadata> SourceMetadata { get; set; } = new Dictionary<string, ButterMorphDesignerSourceMetadata>();

    /// <summary>
    /// Gets or sets the exported DSL content.
    /// </summary>
    public string DslContent { get; set; } = string.Empty;
}
