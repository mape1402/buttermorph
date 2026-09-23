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
    /// Gets the payload alias used by validation assertions.
    /// </summary>
    string ValidationPayloadAlias { get; }

    /// <summary>
    /// Gets the schema key used by validation assertions.
    /// </summary>
    string ValidationSchemaKey { get; }

    /// <summary>
    /// Gets boolean validation assertions associated with the document.
    /// </summary>
    IReadOnlyCollection<IValidationAssertion> ValidationAssertions { get; }

    /// <summary>
    /// Gets UI and tooling metadata for the transformation document.
    /// </summary>
    IReadOnlyDictionary<string, string> Metadata { get; }
}
