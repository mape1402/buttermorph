namespace ButterMorph.StudioPlayground.Models;

/// <summary>
/// Represents a temporary host-owned mapping setup before ButterMorph saves.
/// </summary>
internal sealed class StudioMappingSetup
{
    /// <summary>
    /// Gets or sets the host-owned identifier used to correlate ButterMorph designers.
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the mapping display name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the target schema host id.
    /// </summary>
    public string TargetSchemaId { get; set; } = string.Empty;

    /// <summary>
    /// Gets the source schema host ids keyed by mapping alias.
    /// </summary>
    public Dictionary<string, string> SourceSchemaIds { get; } = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Gets or sets a value indicating whether ButterMorph can load schemas manually.
    /// </summary>
    public bool ShowSchemaActions { get; set; }
}
