namespace ButterMorph.StudioPlayground.Models;

/// <summary>
/// Represents browser-persisted Studio Playground state posted back into the host runtime.
/// </summary>
internal sealed class StudioStateSnapshot
{
    /// <summary>
    /// Gets or sets custom types.
    /// </summary>
    public IReadOnlyCollection<StudioCustomType> CustomTypes { get; set; } = [];

    /// <summary>
    /// Gets or sets custom fields.
    /// </summary>
    public IReadOnlyCollection<StudioCustomField> CustomFields { get; set; } = [];

    /// <summary>
    /// Gets or sets schemas.
    /// </summary>
    public IReadOnlyCollection<StudioSchema> Schemas { get; set; } = [];

    /// <summary>
    /// Gets or sets mappings.
    /// </summary>
    public IReadOnlyCollection<StudioMapping> Mappings { get; set; } = [];
}
