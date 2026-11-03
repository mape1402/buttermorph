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
    /// Gets or sets source metadata keyed by source name.
    /// </summary>
    public IReadOnlyDictionary<string, ButterMorphDesignerSourceMetadata> SourceMetadata { get; set; } = new Dictionary<string, ButterMorphDesignerSourceMetadata>();

    /// <summary>
    /// Gets or sets a value indicating that <see cref="SourceSchemas"/> and <see cref="TargetSchema"/>
    /// represent the complete current schema catalog, including an intentionally empty source list.
    /// </summary>
    public bool HasSchemaSnapshot { get; set; }

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

    /// <summary>
    /// Gets or sets the host revision for the schemas supplied with this snapshot.
    /// </summary>
    public string SchemaRevision { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the host revision for the persisted mapping document.
    /// </summary>
    public string DesignRevision { get; set; } = string.Empty;
}
