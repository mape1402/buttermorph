namespace ButterMorph.Abstractions;

/// <summary>
/// Represents a transformation expression that builds an ordered node.
/// </summary>
public interface IArrayExpression : ITransformationExpression
{
    /// <summary>
    /// Gets the item expressions.
    /// </summary>
    IReadOnlyCollection<ITransformationExpression> Items { get; }
}
