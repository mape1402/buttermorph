using ButterMorph.Abstractions;

namespace ButterMorph.Core;

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
    /// Gets or sets the transformation mappings.
    /// </summary>
    public IReadOnlyCollection<ITransformationMapping> Mappings { get; set; } = [];
}
