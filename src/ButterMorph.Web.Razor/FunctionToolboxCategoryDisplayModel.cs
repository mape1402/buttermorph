namespace ButterMorph.Web.Razor;

/// <summary>
/// Represents one function category in the designer toolbox.
/// </summary>
public sealed class FunctionToolboxCategoryDisplayModel
{
    /// <summary>
    /// Gets or sets the category name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the category functions.
    /// </summary>
    public IReadOnlyCollection<FunctionToolboxItemDisplayModel> Functions { get; set; } = [];
}
