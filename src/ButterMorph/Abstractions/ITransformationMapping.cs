namespace ButterMorph.Abstractions;

/// <summary>
/// Represents a mapping between a source expression and a target path.
/// </summary>
public interface ITransformationMapping
{
    /// <summary>
    /// Gets the source transformation expression.
    /// </summary>
    ITransformationExpression SourceExpression { get; }

    /// <summary>
    /// Gets the target assignment path.
    /// </summary>
    string TargetPath { get; }
}
