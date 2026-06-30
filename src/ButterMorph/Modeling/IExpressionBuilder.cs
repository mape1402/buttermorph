namespace ButterMorph.Modeling;

using ButterMorph.Abstractions;

/// <summary>
/// Builds transformation expressions for modeling documents.
/// </summary>
public interface IExpressionBuilder
{
    /// <summary>
    /// Creates a path expression.
    /// </summary>
    /// <param name="path">The navigation path.</param>
    /// <returns>The path expression.</returns>
    IPathExpression Path(string path);

    /// <summary>
    /// Creates a scalar literal expression.
    /// </summary>
    /// <param name="dataType">The scalar data type.</param>
    /// <param name="rawValue">The raw scalar value.</param>
    /// <returns>The scalar literal expression.</returns>
    IScalarLiteralExpression Scalar(string dataType, string rawValue);

    /// <summary>
    /// Creates a null scalar literal expression.
    /// </summary>
    /// <returns>The null scalar literal expression.</returns>
    IScalarLiteralExpression NullScalar();

    /// <summary>
    /// Creates a boolean scalar literal expression.
    /// </summary>
    /// <param name="value">The boolean value.</param>
    /// <returns>The boolean scalar literal expression.</returns>
    IScalarLiteralExpression Boolean(bool value);

    /// <summary>
    /// Creates a number scalar literal expression.
    /// </summary>
    /// <param name="rawValue">The raw numeric value.</param>
    /// <returns>The number scalar literal expression.</returns>
    IScalarLiteralExpression Number(string rawValue);

    /// <summary>
    /// Creates a scalar collection literal expression.
    /// </summary>
    /// <param name="values">The scalar values.</param>
    /// <returns>The scalar collection literal expression.</returns>
    IScalarCollectionLiteralExpression ScalarCollection(IReadOnlyCollection<IScalarValue> values);

    /// <summary>
    /// Creates a function call expression.
    /// </summary>
    /// <param name="key">The function key.</param>
    /// <param name="arguments">The function argument expressions.</param>
    /// <returns>The function call expression.</returns>
    IFunctionCallExpression Function(string key, IReadOnlyCollection<ITransformationExpression> arguments);

    /// <summary>
    /// Creates a conditional expression.
    /// </summary>
    /// <param name="condition">The condition expression.</param>
    /// <param name="thenExpression">The expression evaluated when true.</param>
    /// <param name="elseExpression">The expression evaluated when false.</param>
    /// <returns>The conditional expression.</returns>
    IConditionalExpression When(ITransformationExpression condition, ITransformationExpression thenExpression, ITransformationExpression elseExpression);

    /// <summary>
    /// Creates a collection projection expression.
    /// </summary>
    /// <param name="sourceCollection">The source collection expression.</param>
    /// <param name="itemAlias">The item alias.</param>
    /// <param name="body">The body expression.</param>
    /// <returns>The collection projection expression.</returns>
    ICollectionProjectionExpression Project(ITransformationExpression sourceCollection, string itemAlias, ITransformationExpression body);

    /// <summary>
    /// Creates a map-shaped expression.
    /// </summary>
    /// <param name="properties">The property expressions.</param>
    /// <returns>The map-shaped expression.</returns>
    IObjectExpression Object(IReadOnlyCollection<IObjectPropertyExpression> properties);

    /// <summary>
    /// Creates a property expression.
    /// </summary>
    /// <param name="name">The property name.</param>
    /// <param name="expression">The property value expression.</param>
    /// <returns>The property expression.</returns>
    IObjectPropertyExpression Property(string name, ITransformationExpression expression);

    /// <summary>
    /// Creates an ordered expression.
    /// </summary>
    /// <param name="items">The item expressions.</param>
    /// <returns>The ordered expression.</returns>
    IArrayExpression Array(IReadOnlyCollection<ITransformationExpression> items);
}
