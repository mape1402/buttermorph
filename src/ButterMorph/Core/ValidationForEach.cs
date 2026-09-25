namespace ButterMorph.Core;

using ButterMorph.Abstractions;

/// <summary>
/// Represents validation statements executed for every item in a collection.
/// </summary>
public sealed class ValidationForEach : IValidationForEach
{
    /// <summary>
    /// Gets or sets the collection expression to iterate.
    /// </summary>
    public ITransformationExpression SourceExpression { get; set; } = new PathExpression();

    /// <summary>
    /// Gets or sets the item alias available inside the loop.
    /// </summary>
    public string ItemAlias { get; set; } = "item";

    /// <summary>
    /// Gets or sets nested validation statements.
    /// </summary>
    public IReadOnlyCollection<IValidationStatement> Statements { get; set; } = [];
}
