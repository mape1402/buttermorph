namespace ButterMorph.Abstractions;

/// <summary>
/// Represents a parsed transformation document.
/// </summary>
public interface ITransformationDocument : IDslDocument
{
    /// <summary>
    /// Gets the transformation mappings.
    /// </summary>
    IReadOnlyCollection<ITransformationMapping> Mappings { get; }
}
