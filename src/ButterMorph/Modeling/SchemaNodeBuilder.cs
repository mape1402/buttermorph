namespace ButterMorph.Modeling;

using ButterMorph.Abstractions;
using ButterMorph.Core;

/// <summary>
/// Builds schema nodes.
/// </summary>
public sealed class SchemaNodeBuilder : ISchemaNodeBuilder
{
    // Stores the node name.
    private string _name = string.Empty;

    // Stores the node shape.
    private SchemaNodeKind _kind = SchemaNodeKind.Object;

    // Stores the scalar data type.
    private string _dataType = string.Empty;

    // Stores whether the node is required.
    private bool _isRequired;

    // Stores child nodes in insertion order.
    private readonly List<ISchemaNode> _children = [];

    // Stores node metadata.
    private readonly Dictionary<string, string> _metadata = new(StringComparer.Ordinal);

    /// <summary>
    /// Configures the node as map-shaped.
    /// </summary>
    /// <param name="name">The node name.</param>
    /// <returns>The current builder.</returns>
    public ISchemaNodeBuilder Object(string name)
    {
        Guard.NotBlank(name, nameof(name));
        _name = name;
        _kind = SchemaNodeKind.Object;
        _dataType = string.Empty;
        return this;
    }

    /// <summary>
    /// Configures the node as ordered.
    /// </summary>
    /// <param name="name">The node name.</param>
    /// <param name="itemSchema">The item schema.</param>
    /// <returns>The current builder.</returns>
    public ISchemaNodeBuilder Array(string name, ISchemaNode itemSchema)
    {
        Guard.NotBlank(name, nameof(name));
        _name = name;
        _kind = SchemaNodeKind.Array;
        _dataType = string.Empty;
        _children.Clear();
        _children.Add(itemSchema);
        return this;
    }

    /// <summary>
    /// Configures the node as scalar.
    /// </summary>
    /// <param name="name">The node name.</param>
    /// <param name="dataType">The scalar data type.</param>
    /// <returns>The current builder.</returns>
    public ISchemaNodeBuilder Scalar(string name, string dataType)
    {
        Guard.NotBlank(name, nameof(name));
        Guard.NotBlank(dataType, nameof(dataType));
        _name = name;
        _kind = SchemaNodeKind.Scalar;
        _dataType = dataType;
        _children.Clear();
        return this;
    }

    /// <summary>
    /// Marks the node as required.
    /// </summary>
    /// <returns>The current builder.</returns>
    public ISchemaNodeBuilder Required()
    {
        _isRequired = true;
        return this;
    }

    /// <summary>
    /// Adds a child schema node.
    /// </summary>
    /// <param name="child">The child schema node.</param>
    /// <returns>The current builder.</returns>
    public ISchemaNodeBuilder WithChild(ISchemaNode child)
    {
        _children.Add(child);
        return this;
    }

    /// <summary>
    /// Adds node metadata.
    /// </summary>
    /// <param name="key">The metadata key.</param>
    /// <param name="value">The metadata value.</param>
    /// <returns>The current builder.</returns>
    public ISchemaNodeBuilder WithMetadata(string key, string value)
    {
        Guard.NotBlank(key, nameof(key));
        _metadata[key] = value;
        return this;
    }

    /// <summary>
    /// Builds the schema node.
    /// </summary>
    /// <returns>The schema node.</returns>
    public ISchemaNode Build()
    {
        Guard.NotBlank(_name, nameof(_name));

        return new SchemaNode
        {
            Name = _name,
            Kind = _kind,
            DataType = _dataType,
            IsRequired = _isRequired,
            Children = [.. _children],
            Metadata = new Dictionary<string, string>(_metadata, StringComparer.Ordinal)
        };
    }
}
