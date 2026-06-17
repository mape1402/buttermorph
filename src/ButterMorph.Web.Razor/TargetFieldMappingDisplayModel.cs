namespace ButterMorph.Web.Razor;

using ButterMorph.Abstractions;

/// <summary>
/// Represents a target schema field with its editable mapping expression.
/// </summary>
public sealed class TargetFieldMappingDisplayModel
{
    /// <summary>
    /// Gets or sets the target path.
    /// </summary>
    public string TargetPath { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the display name.
    /// </summary>
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the target node kind.
    /// </summary>
    public SchemaNodeKind Kind { get; set; }

    /// <summary>
    /// Gets or sets the target data type.
    /// </summary>
    public string DataType { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the current expression text.
    /// </summary>
    public string Expression { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the suggested expression placeholder.
    /// </summary>
    public string Placeholder { get; set; } = string.Empty;
}
