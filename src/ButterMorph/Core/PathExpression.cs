namespace ButterMorph.Core;

using ButterMorph.Abstractions;

/// <summary>
/// Represents a transformation expression that resolves a navigation path.
/// </summary>
public sealed class PathExpression : IPathExpression
{
    /// <summary>
    /// Gets the expression kind.
    /// </summary>
    public TransformationExpressionKind Kind => TransformationExpressionKind.Path;

    /// <summary>
    /// Gets or sets the navigation path.
    /// </summary>
    public string Path { get; set; } = string.Empty;
}
