namespace ButterMorph.Abstractions;

/// <summary>
/// Represents one named property in a map-shaped transformation expression.
/// </summary>
public interface IObjectPropertyExpression
{
    /// <summary>
    /// Gets the property name.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Gets the property value expression.
    /// </summary>
    ITransformationExpression Expression { get; }
}
