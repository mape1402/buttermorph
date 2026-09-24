namespace ButterMorph.Web.Razor;

using ButterMorph.Abstractions;

/// <summary>
/// Represents host-provided validation designer state.
/// </summary>
public sealed class ButterMorphValidationDesignerLoadResult
{
    /// <summary>
    /// Gets or sets the source schemas keyed by source name.
    /// </summary>
    public IReadOnlyDictionary<string, IStructureSchema> SourceSchemas { get; set; } = new Dictionary<string, IStructureSchema>();

    /// <summary>
    /// Gets or sets source metadata keyed by source name.
    /// </summary>
    public IReadOnlyDictionary<string, ButterMorphDesignerSourceMetadata> SourceMetadata { get; set; } = new Dictionary<string, ButterMorphDesignerSourceMetadata>();

    /// <summary>
    /// Gets or sets the initial validation document.
    /// </summary>
    public IValidationDocument InitialDocument { get; set; }

    /// <summary>
    /// Gets or sets initial DSL content used to hydrate the validation document.
    /// </summary>
    public string InitialDslContent { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the optional load message.
    /// </summary>
    public string Message { get; set; } = string.Empty;
}
