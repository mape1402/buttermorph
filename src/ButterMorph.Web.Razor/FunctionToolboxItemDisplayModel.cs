namespace ButterMorph.Web.Razor;

using ButterMorph.Abstractions;

/// <summary>
/// Represents one function item in the designer toolbox.
/// </summary>
public sealed class FunctionToolboxItemDisplayModel
{
    /// <summary>
    /// Gets or sets the function key.
    /// </summary>
    public string Key { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the display name.
    /// </summary>
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the function description.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the function category.
    /// </summary>
    public string Category { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the return value kind.
    /// </summary>
    public FunctionValueKind ValueKind { get; set; }

    /// <summary>
    /// Gets or sets the expression template inserted into mappings.
    /// </summary>
    public string Template { get; set; } = string.Empty;
}
