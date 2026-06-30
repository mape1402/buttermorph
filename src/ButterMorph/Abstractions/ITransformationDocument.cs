namespace ButterMorph.Abstractions;

/// <summary>
/// Represents a parsed transformation document.
/// </summary>
public interface ITransformationDocument : IDslDocument
{
    /// <summary>
    /// Gets the source schemas keyed by source name.
    /// </summary>
    IReadOnlyDictionary<string, IStructureSchema> SourceSchemas { get; }

    /// <summary>
    /// Gets the target schema.
    /// </summary>
    IStructureSchema TargetSchema { get; }

    /// <summary>
    /// Gets the transformation mappings.
    /// </summary>
    IReadOnlyCollection<ITransformationMapping> Mappings { get; }

    /// <summary>
    /// Gets validation rules associated with the target graph.
    /// </summary>
    IReadOnlyCollection<IValidationRule> Validations { get; }

    /// <summary>
    /// Gets UI and tooling metadata for the transformation document.
    /// </summary>
    IReadOnlyDictionary<string, string> Metadata { get; }
}
