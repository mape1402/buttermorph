namespace ButterMorph.Abstractions;

/// <summary>
/// Represents a validation execution request.
/// </summary>
public sealed class ValidationRequest
{
    /// <summary>
    /// Gets or sets the graph to validate.
    /// </summary>
    public IStructureGraph SourceGraph { get; set; }

    /// <summary>
    /// Gets or sets the parsed validation definition.
    /// </summary>
    public IDslDocument Definition { get; set; }
}
