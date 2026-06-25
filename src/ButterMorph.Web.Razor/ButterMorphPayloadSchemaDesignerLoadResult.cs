namespace ButterMorph.Web.Razor;

using ButterMorph.SchemaDesign;

/// <summary>
/// Represents schema designer load state.
/// </summary>
public sealed class ButterMorphPayloadSchemaDesignerLoadResult
{
    /// <summary>
    /// Gets or sets the canonical schema key.
    /// </summary>
    public string Key { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the display schema name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the schema description.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the schema version.
    /// </summary>
    public string Version { get; set; } = "1.0.0";

    /// <summary>
    /// Gets or sets the schema version comment.
    /// </summary>
    public string VersionComment { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets open schema metadata.
    /// </summary>
    public IReadOnlyDictionary<string, string> Metadata { get; set; } = new Dictionary<string, string>();

    /// <summary>
    /// Gets or sets the host-provided schema metadata definition.
    /// </summary>
    public SchemaMetadataDefinition MetadataDefinition { get; set; } = new();

    /// <summary>
    /// Gets or sets the payload JSON Schema.
    /// </summary>
    public string JsonSchema { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the available schema type catalog.
    /// </summary>
    public IReadOnlyCollection<SchemaTypeCatalogItem> SchemaTypes { get; set; } = [];

    /// <summary>
    /// Gets or sets the available field metadata catalog.
    /// </summary>
    public IReadOnlyCollection<FieldMetadataCatalogItem> MetadataFields { get; set; } = [];

    /// <summary>
    /// Gets or sets a value indicating whether manual actions are shown.
    /// </summary>
    public bool ShowManualActions { get; set; } = true;

    /// <summary>
    /// Gets or sets an optional user-facing message.
    /// </summary>
    public string Message { get; set; } = string.Empty;
}
