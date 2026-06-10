namespace ButterMorph.Abstractions;

/// <summary>
/// Represents a transformation execution request.
/// </summary>
public sealed class TransformationRequest
{
    /// <summary>
    /// Gets or sets the source graphs used by the transformation.
    /// </summary>
    public IDictionary<string, IStructureGraph> Sources { get; set; } = new Dictionary<string, IStructureGraph>();

    /// <summary>
    /// Gets or sets the transformation definition.
    /// </summary>
    public string Definition { get; set; } = string.Empty;
}
