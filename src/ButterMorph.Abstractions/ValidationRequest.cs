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
    /// Gets or sets the validation definition.
    /// </summary>
    public string Definition { get; set; } = string.Empty;
}
