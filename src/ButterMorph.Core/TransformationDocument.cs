namespace ButterMorph.Core;

using ButterMorph.Abstractions;

/// <summary>
/// Represents a parsed transformation document.
/// </summary>
public sealed class TransformationDocument : ITransformationDocument
{
    /// <summary>
    /// Gets or sets the source DSL definition.
    /// </summary>
    public IDslDefinition Definition { get; set; }

    /// <summary>
    /// Gets or sets the source schemas keyed by source name.
    /// </summary>
    public IReadOnlyDictionary<string, IStructureSchema> SourceSchemas { get; set; } = new Dictionary<string, IStructureSchema>();

    /// <summary>
    /// Gets or sets the target schema.
    /// </summary>
    public IStructureSchema TargetSchema { get; set; } = new StructureSchema();

    /// <summary>
    /// Gets or sets the transformation mappings.
    /// </summary>
    public IReadOnlyCollection<ITransformationMapping> Mappings { get; set; } = [];

    /// <summary>
    /// Gets or sets validation rules associated with the target graph.
    /// </summary>
    public IReadOnlyCollection<IValidationRule> Validations { get; set; } = [];

    /// <summary>
    /// Gets or sets UI and tooling metadata for the transformation document.
    /// </summary>
    public IReadOnlyDictionary<string, string> Metadata { get; set; } = new Dictionary<string, string>();
}
