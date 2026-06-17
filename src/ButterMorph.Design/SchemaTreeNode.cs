namespace ButterMorph.Design;

using ButterMorph.Abstractions;

/// <summary>
/// Represents a schema node prepared for design-time navigation.
/// </summary>
public sealed class SchemaTreeNode : ISchemaTreeNode
{
    /// <summary>
    /// Gets or sets the schema node name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the design-time path.
    /// </summary>
    public string Path { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the schema node kind.
    /// </summary>
    public SchemaNodeKind Kind { get; set; }

    /// <summary>
    /// Gets or sets the scalar data type.
    /// </summary>
    public string DataType { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a value indicating whether the node is required.
    /// </summary>
    public bool IsRequired { get; set; }

    /// <summary>
    /// Gets or sets node metadata.
    /// </summary>
    public IReadOnlyDictionary<string, string> Metadata { get; set; } = new Dictionary<string, string>();

    /// <summary>
    /// Gets or sets child nodes.
    /// </summary>
    public IReadOnlyCollection<ISchemaTreeNode> Children { get; set; } = [];
}
