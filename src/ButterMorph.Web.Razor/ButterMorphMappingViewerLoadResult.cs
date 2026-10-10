namespace ButterMorph.Web.Razor;

using ButterMorph.Abstractions;

/// <summary>
/// Describes host-provided mapping viewer content.
/// </summary>
public sealed class ButterMorphMappingViewerLoadResult
{
    /// <summary>
    /// Gets or sets the mapping key.
    /// </summary>
    public string Key { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the mapping display name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the mapping description.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the mapping version.
    /// </summary>
    public string Version { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the parsed mapping document.
    /// </summary>
    public ITransformationDocument Document { get; set; }

    /// <summary>
    /// Gets or sets the target schema used by the mapping.
    /// </summary>
    public IStructureSchema TargetSchema { get; set; }

    /// <summary>
    /// Gets or sets the raw DSL content.
    /// </summary>
    public string DslContent { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a user-facing message.
    /// </summary>
    public string Message { get; set; } = string.Empty;
}
