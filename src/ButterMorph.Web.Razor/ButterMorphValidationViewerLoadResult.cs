namespace ButterMorph.Web.Razor;

using ButterMorph.Abstractions;

/// <summary>
/// Describes host-provided validation viewer content.
/// </summary>
public sealed class ButterMorphValidationViewerLoadResult
{
    /// <summary>
    /// Gets or sets the validation key.
    /// </summary>
    public string Key { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the validation display name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the validation description.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the validation version.
    /// </summary>
    public string Version { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the parsed validation document.
    /// </summary>
    public IValidationDocument Document { get; set; }

    /// <summary>
    /// Gets or sets the raw DSL content.
    /// </summary>
    public string DslContent { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a user-facing message.
    /// </summary>
    public string Message { get; set; } = string.Empty;
}
