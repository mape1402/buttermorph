namespace ButterMorph.Web.Razor;

/// <summary>
/// Represents design-time metadata for one source schema alias.
/// </summary>
public sealed class ButterMorphDesignerSourceMetadata
{
    /// <summary>
    /// Gets or sets the source display name shown in the designer.
    /// </summary>
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the source description shown in hover details.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets source tags shown in hover details.
    /// </summary>
    public IReadOnlyCollection<string> Tags { get; set; } = [];
}
