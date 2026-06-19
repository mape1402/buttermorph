namespace ButterMorph.Web.Razor;

using ButterMorph.Abstractions;

/// <summary>
/// Represents a flattened schema node for Razor rendering.
/// </summary>
public sealed class SchemaTreeDisplayNode
{
    /// <summary>
    /// Gets or sets the display depth.
    /// </summary>
    public int Depth { get; set; }

    /// <summary>
    /// Gets or sets the design-time path.
    /// </summary>
    public string Path { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the schema node name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the schema node kind.
    /// </summary>
    public SchemaNodeKind Kind { get; set; }

    /// <summary>
    /// Gets or sets the scalar data type.
    /// </summary>
    public string DataType { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets child display nodes.
    /// </summary>
    public IReadOnlyCollection<SchemaTreeDisplayNode> Children { get; set; } = [];

    /// <summary>
    /// Gets or sets a value indicating whether the node is expanded by default.
    /// </summary>
    public bool IsExpanded { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the node can be dragged.
    /// </summary>
    public bool CanDrag { get; set; }

    /// <summary>
    /// Gets or sets the mapping expression.
    /// </summary>
    public string Expression { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the expression placeholder.
    /// </summary>
    public string Placeholder { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets diagnostics for this node.
    /// </summary>
    public IReadOnlyCollection<string> Diagnostics { get; set; } = [];

    /// <summary>
    /// Gets or sets a value indicating whether this node edits an array projection.
    /// </summary>
    public bool IsArrayProjection { get; set; }

    /// <summary>
    /// Gets or sets the array projection source expression.
    /// </summary>
    public string ProjectionSourceExpression { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the array projection alias.
    /// </summary>
    public string ProjectionAlias { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets an advanced projection expression when it cannot be decomposed.
    /// </summary>
    public string ProjectionAdvancedExpression { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a value indicating whether this node is inside an array template.
    /// </summary>
    public bool IsArrayTemplateField { get; set; }

    /// <summary>
    /// Gets or sets the array projection target path for a template field.
    /// </summary>
    public string ProjectionTargetPath { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the relative projection field path.
    /// </summary>
    public string ProjectionFieldPath { get; set; } = string.Empty;
}
