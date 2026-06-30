namespace ButterMorph.Abstractions;

/// <summary>
/// Defines the kind of transformation expression.
/// </summary>
public enum TransformationExpressionKind
{
    /// <summary>
    /// Represents an expression that resolves a navigation path.
    /// </summary>
    Path,

    /// <summary>
    /// Represents an expression that contains a scalar literal.
    /// </summary>
    ScalarLiteral,

    /// <summary>
    /// Represents an expression that contains scalar literal values.
    /// </summary>
    ScalarCollectionLiteral,

    /// <summary>
    /// Represents an expression that calls a registered function.
    /// </summary>
    FunctionCall,

    /// <summary>
    /// Represents an expression that selects between two branches.
    /// </summary>
    Conditional,

    /// <summary>
    /// Represents an expression that projects a source collection.
    /// </summary>
    CollectionProjection,

    /// <summary>
    /// Represents an expression that builds a map-shaped node.
    /// </summary>
    Object,

    /// <summary>
    /// Represents an expression that builds an ordered node.
    /// </summary>
    Array
}
