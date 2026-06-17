namespace ButterMorph.Core;

using ButterMorph.Abstractions;

/// <summary>
/// Represents a mapping between a source expression and a target path.
/// </summary>
public sealed class TransformationMapping : ITransformationMapping
{
    /// <summary>
    /// Gets or sets the source transformation expression.
    /// </summary>
    public ITransformationExpression SourceExpression { get; set; } = new PathExpression();

    /// <summary>
    /// Gets or sets the target assignment path.
    /// </summary>
    public string TargetPath { get; set; } = string.Empty;
}
