namespace ButterMorph.Web.Razor;

/// <summary>
/// Represents a source schema group in the designer toolbox.
/// </summary>
public sealed class SourceSchemaDisplayModel
{
    /// <summary>
    /// Gets or sets the source key.
    /// </summary>
    public string Key { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the canonical schema key.
    /// </summary>
    public string SchemaKey { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the schema display name.
    /// </summary>
    public string SchemaName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the schema version.
    /// </summary>
    public string Version { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the schema topic metadata.
    /// </summary>
    public string Topic { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the source schema root node.
    /// </summary>
    public SchemaTreeDisplayNode Root { get; set; } = new();
}
