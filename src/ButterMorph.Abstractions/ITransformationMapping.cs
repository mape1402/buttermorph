namespace ButterMorph.Abstractions;

/// <summary>
/// Represents a mapping between a source path and a target path.
/// </summary>
public interface ITransformationMapping
{
    /// <summary>
    /// Gets the source navigation path.
    /// </summary>
    string SourcePath { get; }

    /// <summary>
    /// Gets the target assignment path.
    /// </summary>
    string TargetPath { get; }
}
