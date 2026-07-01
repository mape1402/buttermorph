namespace ButterMorph.StudioPlayground.Models;

using System.Text.Json.Serialization;
using ButterMorph.Abstractions;

/// <summary>
/// Represents a host-owned mapping document.
/// </summary>
internal sealed class StudioMapping
{
    /// <summary>
    /// Gets or sets the host-owned identifier used to correlate ButterMorph designers.
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the display name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the target schema host id.
    /// </summary>
    public string TargetSchemaId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the source schema host ids keyed by mapping alias.
    /// </summary>
    public Dictionary<string, string> SourceSchemaIds { get; set; } = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Gets or sets the sample JSON payloads keyed by source alias.
    /// </summary>
    public Dictionary<string, string> SourceSamples { get; set; } = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Gets or sets a value indicating whether ButterMorph can load schemas manually.
    /// </summary>
    public bool ShowSchemaActions { get; set; }

    /// <summary>
    /// Gets or sets the transformation document.
    /// </summary>
    [JsonIgnore]
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

