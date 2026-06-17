namespace ButterMorph.Design;

using ButterMorph.Abstractions;

/// <summary>
/// Represents a schema node prepared for design-time navigation.
/// </summary>
public interface ISchemaTreeNode
{
    /// <summary>
    /// Gets the schema node name.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Gets the design-time path.
    /// </summary>
    string Path { get; }

    /// <summary>
    /// Gets the schema node kind.
    /// </summary>
    SchemaNodeKind Kind { get; }

    /// <summary>
    /// Gets the scalar data type.
    /// </summary>
    string DataType { get; }

    /// <summary>
    /// Gets a value indicating whether the node is required.
    /// </summary>
    bool IsRequired { get; }

    /// <summary>
    /// Gets node metadata.
    /// </summary>
    IReadOnlyDictionary<string, string> Metadata { get; }

    /// <summary>
    /// Gets child nodes.
    /// </summary>
    IReadOnlyCollection<ISchemaTreeNode> Children { get; }
}
