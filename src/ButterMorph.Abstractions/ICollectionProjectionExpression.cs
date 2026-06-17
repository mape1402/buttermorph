namespace ButterMorph.Abstractions;

/// <summary>
/// Represents a transformation expression that projects a source collection.
/// </summary>
public interface ICollectionProjectionExpression : ITransformationExpression
{
    /// <summary>
    /// Gets the source collection expression.
    /// </summary>
    ITransformationExpression SourceExpression { get; }

    /// <summary>
    /// Gets the alias used for each item while evaluating the body.
    /// </summary>
    string ItemAlias { get; }

    /// <summary>
    /// Gets the body expression evaluated for each item.
    /// </summary>
    ITransformationExpression BodyExpression { get; }
}
