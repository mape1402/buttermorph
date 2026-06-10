namespace ButterMorph.Abstractions;

/// <summary>
/// Represents a transformation execution request.
/// </summary>
public sealed class TransformationRequest
{
    /// <summary>
    /// Gets or sets the source graphs used by the transformation.
    /// </summary>
    public IReadOnlyDictionary<string, IStructureGraph> Sources { get; set; } = new Dictionary<string, IStructureGraph>();

    /// <summary>
    /// Gets or sets the parsed transformation definition.
    /// </summary>
    public IDslDocument Definition { get; set; }
}
