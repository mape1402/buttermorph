namespace ButterMorph.Web.Razor;

/// <summary>
/// Represents an editable array projection in the Razor designer.
/// </summary>
public sealed class ArrayProjectionDisplayModel
{
    /// <summary>
    /// Gets or sets the target array path.
    /// </summary>
    public string TargetPath { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the source collection expression.
    /// </summary>
    public string SourceExpression { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the item alias.
    /// </summary>
    public string Alias { get; set; } = "item";

    /// <summary>
    /// Gets or sets an advanced projection expression.
    /// </summary>
    public string AdvancedExpression { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets field expressions by relative item field path.
    /// </summary>
    public IReadOnlyDictionary<string, string> FieldExpressions { get; set; } = new Dictionary<string, string>();

    /// <summary>
    /// Gets or sets visual mapping states by relative item field path.
    /// </summary>
    public IReadOnlyDictionary<string, ConditionalMappingDisplayModel> FieldMappings { get; set; } = new Dictionary<string, ConditionalMappingDisplayModel>();
}
