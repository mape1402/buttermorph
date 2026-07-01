namespace ButterMorph.Web.Razor;

using ButterMorph.Abstractions;

/// <summary>
/// Represents host-provided designer state.
/// </summary>
public sealed class ButterMorphDesignerLoadResult
{
    /// <summary>
    /// Gets or sets the source schemas keyed by source name.
    /// </summary>
    public IReadOnlyDictionary<string, IStructureSchema> SourceSchemas { get; set; } = new Dictionary<string, IStructureSchema>();

    /// <summary>
    /// Gets or sets the target schema.
    /// </summary>
    public IStructureSchema TargetSchema { get; set; }

    /// <summary>
    /// Gets or sets the initial transformation document.
    /// </summary>
    public ITransformationDocument InitialDocument { get; set; }

    /// <summary>
    /// Gets or sets initial DSL content used to hydrate the mapping document.
    /// </summary>
    public string InitialDslContent { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a value indicating whether schema action buttons should be shown.
    /// </summary>
    public bool ShowSchemaActions { get; set; } = true;

    /// <summary>
    /// Gets or sets the optional load message.
    /// </summary>
    public string Message { get; set; } = string.Empty;
}
