using ButterMorph.Abstractions;

namespace ButterMorph.Core;

/// <summary>
/// Represents a mapping between a source path and a target path.
/// </summary>
public sealed class TransformationMapping : ITransformationMapping
{
    /// <summary>
    /// Gets or sets the source navigation path.
    /// </summary>
    public string SourcePath { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the target assignment path.
    /// </summary>
    public string TargetPath { get; set; } = string.Empty;
}
