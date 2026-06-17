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
}
