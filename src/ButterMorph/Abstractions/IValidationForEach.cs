namespace ButterMorph.Abstractions;

/// <summary>
/// Represents validation statements executed for every item in a collection.
/// </summary>
public interface IValidationForEach : IValidationStatement
{
    /// <summary>
    /// Gets the collection expression to iterate.
    /// </summary>
    ITransformationExpression SourceExpression { get; }

    /// <summary>
    /// Gets the item alias available inside the loop.
    /// </summary>
    string ItemAlias { get; }

    /// <summary>
    /// Gets nested validation statements.
    /// </summary>
    IReadOnlyCollection<IValidationStatement> Statements { get; }
}
