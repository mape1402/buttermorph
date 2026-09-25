namespace ButterMorph.Web.Razor;

/// <summary>
/// Represents an editable validation foreach block.
/// </summary>
public sealed class ValidationForEachDisplayModel
{
    /// <summary>
    /// Gets or sets the source collection expression.
    /// </summary>
    public string SourceExpression { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the item alias.
    /// </summary>
    public string Alias { get; set; } = "item";

    /// <summary>
    /// Gets or sets the nested assertions.
    /// </summary>
    public IReadOnlyCollection<ValidationAssertionDisplayModel> Assertions { get; set; } = [];
}
