namespace ButterMorph.Abstractions;

/// <summary>
/// Represents one node in a structure schema.
/// </summary>
public interface ISchemaNode
{
    /// <summary>
    /// Gets the schema node name.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Gets the structural schema node kind.
    /// </summary>
    SchemaNodeKind Kind { get; }

    /// <summary>
    /// Gets the scalar data type expected by the node.
    /// </summary>
    string DataType { get; }

    /// <summary>
    /// Gets a value indicating whether the node is required.
    /// </summary>
    bool IsRequired { get; }

    /// <summary>
    /// Gets the child schema nodes.
    /// </summary>
    IReadOnlyCollection<ISchemaNode> Children { get; }

    /// <summary>
    /// Gets UI and tooling metadata for the node.
    /// </summary>
    IReadOnlyDictionary<string, string> Metadata { get; }
}
