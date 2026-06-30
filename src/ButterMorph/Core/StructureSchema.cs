namespace ButterMorph.Core;

using ButterMorph.Abstractions;

/// <summary>
/// Represents a structure schema used by visual tooling and DSL compilation.
/// </summary>
public sealed class StructureSchema : IStructureSchema
{
    /// <summary>
    /// Gets or sets the canonical schema key.
    /// </summary>
    public string Key { get; set; } = "schema";

    /// <summary>
    /// Gets or sets the schema name.
    /// </summary>
    public string Name { get; set; } = "Schema";

    /// <summary>
    /// Gets or sets the schema description.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the concrete schema version.
    /// </summary>
    public string Version { get; set; } = "1.0.0";

    /// <summary>
    /// Gets or sets the schema version comment.
    /// </summary>
    public string VersionComment { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the schema root node.
    /// </summary>
    public ISchemaNode Root { get; set; } = new SchemaNode();

    /// <summary>
    /// Gets or sets UI and tooling metadata for the schema.
    /// </summary>
    public IReadOnlyDictionary<string, string> Metadata { get; set; } = new Dictionary<string, string>();
}
