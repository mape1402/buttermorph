namespace ButterMorph.Web.Razor;

/// <summary>
/// Represents a mapping row for Razor rendering.
/// </summary>
public sealed class MappingDisplayModel
{
    /// <summary>
    /// Gets or sets the target path.
    /// </summary>
    public string TargetPath { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the expression text.
    /// </summary>
    public string Expression { get; set; } = string.Empty;
}
