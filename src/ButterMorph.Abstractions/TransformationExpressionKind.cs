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
    /// Represents an expression that calls a registered function.
    /// </summary>
    FunctionCall
}
