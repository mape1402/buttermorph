namespace ButterMorph.Abstractions;

/// <summary>
/// Represents a transformation expression that builds a map-shaped node.
/// </summary>
public interface IObjectExpression : ITransformationExpression
{
    /// <summary>
    /// Gets the property expressions.
    /// </summary>
    IReadOnlyCollection<IObjectPropertyExpression> Properties { get; }
}
