namespace ButterMorph.Core;

using ButterMorph.Abstractions;

/// <summary>
/// Represents one named property in a map-shaped transformation expression.
/// </summary>
public sealed class ObjectPropertyExpression : IObjectPropertyExpression
{
    /// <summary>
    /// Gets or sets the property name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the property value expression.
    /// </summary>
    public ITransformationExpression Expression { get; set; } = new PathExpression();
}
