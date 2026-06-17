namespace ButterMorph.Modeling;

using ButterMorph.Abstractions;

/// <summary>
/// Builds schema nodes.
/// </summary>
public interface ISchemaNodeBuilder
{
    /// <summary>
    /// Configures the node as map-shaped.
    /// </summary>
    /// <param name="name">The node name.</param>
    /// <returns>The current builder.</returns>
    ISchemaNodeBuilder Object(string name);

    /// <summary>
    /// Configures the node as ordered.
    /// </summary>
    /// <param name="name">The node name.</param>
    /// <param name="itemSchema">The item schema.</param>
    /// <returns>The current builder.</returns>
    ISchemaNodeBuilder Array(string name, ISchemaNode itemSchema);

    /// <summary>
    /// Configures the node as scalar.
    /// </summary>
    /// <param name="name">The node name.</param>
    /// <param name="dataType">The scalar data type.</param>
    /// <returns>The current builder.</returns>
    ISchemaNodeBuilder Scalar(string name, string dataType);

    /// <summary>
    /// Marks the node as required.
    /// </summary>
    /// <returns>The current builder.</returns>
    ISchemaNodeBuilder Required();

    /// <summary>
    /// Adds a child schema node.
    /// </summary>
    /// <param name="child">The child schema node.</param>
    /// <returns>The current builder.</returns>
    ISchemaNodeBuilder WithChild(ISchemaNode child);

    /// <summary>
    /// Adds node metadata.
    /// </summary>
    /// <param name="key">The metadata key.</param>
    /// <param name="value">The metadata value.</param>
    /// <returns>The current builder.</returns>
    ISchemaNodeBuilder WithMetadata(string key, string value);

    /// <summary>
    /// Builds the schema node.
    /// </summary>
    /// <returns>The schema node.</returns>
    ISchemaNode Build();
}
