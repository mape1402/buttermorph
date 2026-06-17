namespace ButterMorph.Core;

using ButterMorph.Abstractions;

/// <summary>
/// Represents one node in a structure schema.
/// </summary>
public sealed class SchemaNode : ISchemaNode
{
    /// <summary>
    /// Gets or sets the schema node name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the structural schema node kind.
    /// </summary>
    public SchemaNodeKind Kind { get; set; }

    /// <summary>
    /// Gets or sets the scalar data type expected by the node.
    /// </summary>
    public string DataType { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a value indicating whether the node is required.
    /// </summary>
    public bool IsRequired { get; set; }

    /// <summary>
    /// Gets or sets the child schema nodes.
    /// </summary>
    public IReadOnlyCollection<ISchemaNode> Children { get; set; } = [];

    /// <summary>
    /// Gets or sets UI and tooling metadata for the node.
    /// </summary>
    public IReadOnlyDictionary<string, string> Metadata { get; set; } = new Dictionary<string, string>();
}
