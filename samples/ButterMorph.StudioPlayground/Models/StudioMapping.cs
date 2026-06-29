namespace ButterMorph.StudioPlayground.Models;

using ButterMorph.Abstractions;

/// <summary>
/// Represents a host-owned mapping document.
/// </summary>
internal sealed class StudioMapping
{
    /// <summary>
    /// Gets or sets the context key used by ButterMorph designers.
    /// </summary>
    public string ContextKey { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the display name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the target schema context key.
    /// </summary>
    public string TargetSchemaKey { get; set; } = string.Empty;

    /// <summary>
    /// Gets the source schema context keys.
    /// </summary>
    public Dictionary<string, string> SourceSchemaKeys { get; } = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Gets the sample JSON payloads keyed by source alias.
    /// </summary>
    public Dictionary<string, string> SourceSamples { get; } = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Gets or sets the transformation document.
    /// </summary>
    public ITransformationDocument Document { get; set; }

    /// <summary>
    /// Gets or sets the exported DSL.
    /// </summary>
    public string DslContent { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the last saved timestamp.
    /// </summary>
    public DateTimeOffset SavedAt { get; set; } = DateTimeOffset.UtcNow;
}
