namespace ButterMorph.Web.Razor;

/// <summary>
/// Defines schema metadata fields supplied by the host.
/// </summary>
public sealed class SchemaMetadataDefinition
{
    /// <summary>
    /// Gets or sets the metadata fields.
    /// </summary>
    public IReadOnlyCollection<SchemaMetadataFieldDefinition> Fields { get; set; } = [];
}
