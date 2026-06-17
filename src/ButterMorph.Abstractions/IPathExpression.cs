namespace ButterMorph.Abstractions;

/// <summary>
/// Represents a transformation expression that resolves a navigation path.
/// </summary>
public interface IPathExpression : ITransformationExpression
{
    /// <summary>
    /// Gets the navigation path.
    /// </summary>
    string Path { get; }
}
