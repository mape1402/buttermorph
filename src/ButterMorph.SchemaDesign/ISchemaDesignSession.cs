namespace ButterMorph.SchemaDesign;

using ButterMorph.Abstractions;

/// <summary>
/// Defines editable schema design behavior.
/// </summary>
public interface ISchemaDesignSession
{
    /// <summary>
    /// Gets the current schema.
    /// </summary>
    IStructureSchema Schema { get; }

    /// <summary>
    /// Loads an existing schema into the session.
    /// </summary>
    /// <param name="schema">The schema to load.</param>
    /// <returns>The operation result.</returns>
    ISchemaDesignOperationResult Load(IStructureSchema schema);

    /// <summary>
    /// Imports JSON Schema text into the session.
    /// </summary>
    /// <param name="name">The schema name.</param>
    /// <param name="jsonSchema">The JSON Schema text.</param>
    /// <returns>The operation result.</returns>
    ISchemaDesignOperationResult ImportJsonSchema(string name, string jsonSchema);

    /// <summary>
    /// Exports the current schema as JSON Schema text.
    /// </summary>
    /// <returns>The exported JSON Schema text.</returns>
    string ExportJsonSchema();

    /// <summary>
    /// Adds a node under the selected parent path.
    /// </summary>
    /// <param name="parentPath">The parent path.</param>
    /// <param name="name">The new node name.</param>
    /// <param name="kind">The new node kind.</param>
    /// <param name="dataType">The scalar data type.</param>
    /// <returns>The operation result.</returns>
    ISchemaDesignOperationResult AddNode(string parentPath, string name, SchemaNodeKind kind, string dataType);

    /// <summary>
    /// Updates a node.
    /// </summary>
    /// <param name="path">The node path.</param>
    /// <param name="name">The node name.</param>
    /// <param name="kind">The node kind.</param>
    /// <param name="dataType">The scalar data type.</param>
    /// <param name="isRequired">A value indicating whether the node is required.</param>
    /// <returns>The operation result.</returns>
    ISchemaDesignOperationResult UpdateNode(string path, string name, SchemaNodeKind kind, string dataType, bool isRequired);

    /// <summary>
    /// Removes a node.
    /// </summary>
    /// <param name="path">The node path.</param>
    /// <returns>The operation result.</returns>
    ISchemaDesignOperationResult RemoveNode(string path);

    /// <summary>
    /// Sets metadata on a schema node.
    /// </summary>
    /// <param name="path">The node path.</param>
    /// <param name="key">The metadata key.</param>
    /// <param name="value">The metadata value.</param>
    /// <returns>The operation result.</returns>
    ISchemaDesignOperationResult SetMetadata(string path, string key, string value);
}
