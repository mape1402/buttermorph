namespace ButterMorph.Modeling;

using ButterMorph.Abstractions;
using ButterMorph.Core;

/// <summary>
/// Builds transformation expressions for modeling documents.
/// </summary>
public sealed class ExpressionBuilder : IExpressionBuilder
{
    /// <summary>
    /// Creates a path expression.
    /// </summary>
    /// <param name="path">The navigation path.</param>
    /// <returns>The path expression.</returns>
    public IPathExpression Path(string path)
    {
        Guard.NotBlank(path, nameof(path));

        return new PathExpression
        {
            Path = path
        };
    }

    /// <summary>
    /// Creates a scalar literal expression.
    /// </summary>
    /// <param name="dataType">The scalar data type.</param>
    /// <param name="rawValue">The raw scalar value.</param>
    /// <returns>The scalar literal expression.</returns>
    public IScalarLiteralExpression Scalar(string dataType, string rawValue)
    {
        Guard.NotBlank(dataType, nameof(dataType));

        return new ScalarLiteralExpression
        {
            Value = new ScalarValue
            {
                DataType = dataType,
                RawValue = rawValue,
                IsNull = false
            }
        };
    }

    /// <summary>
    /// Creates a null scalar literal expression.
    /// </summary>
    /// <returns>The null scalar literal expression.</returns>
    public IScalarLiteralExpression NullScalar()
    {
        return new ScalarLiteralExpression
        {
            Value = new ScalarValue
            {
                DataType = "Null",
                RawValue = string.Empty,
                IsNull = true
            }
        };
    }

    /// <summary>
    /// Creates a boolean scalar literal expression.
    /// </summary>
    /// <param name="value">The boolean value.</param>
    /// <returns>The boolean scalar literal expression.</returns>
    public IScalarLiteralExpression Boolean(bool value)
    {
        string rawValue = "false";

        if (value)
        {
            rawValue = "true";
        }

        return new ScalarLiteralExpression
        {
            Value = new ScalarValue
            {
                DataType = "Boolean",
                RawValue = rawValue,
                IsNull = false
            }
        };
    }

    /// <summary>
    /// Creates a number scalar literal expression.
    /// </summary>
    /// <param name="rawValue">The raw numeric value.</param>
    /// <returns>The number scalar literal expression.</returns>
    public IScalarLiteralExpression Number(string rawValue)
    {
        Guard.NotBlank(rawValue, nameof(rawValue));

        return Scalar("Number", rawValue);
    }

    /// <summary>
    /// Creates a scalar collection literal expression.
    /// </summary>
    /// <param name="values">The scalar values.</param>
    /// <returns>The scalar collection literal expression.</returns>
    public IScalarCollectionLiteralExpression ScalarCollection(IReadOnlyCollection<IScalarValue> values)
    {
        Guard.NotEmpty(values, nameof(values));

        return new ScalarCollectionLiteralExpression
        {
            Values = [.. values]
        };
    }

    /// <summary>
    /// Creates a function call expression.
    /// </summary>
    /// <param name="key">The function key.</param>
    /// <param name="arguments">The function argument expressions.</param>
    /// <returns>The function call expression.</returns>
    public IFunctionCallExpression Function(string key, IReadOnlyCollection<ITransformationExpression> arguments)
    {
        Guard.NotBlank(key, nameof(key));

        return new FunctionCallExpression
        {
            FunctionKey = key,
            Arguments = [.. arguments]
        };
    }

    /// <summary>
    /// Creates a conditional expression.
    /// </summary>
    /// <param name="condition">The condition expression.</param>
    /// <param name="thenExpression">The expression evaluated when true.</param>
    /// <param name="elseExpression">The expression evaluated when false.</param>
    /// <returns>The conditional expression.</returns>
    public IConditionalExpression When(ITransformationExpression condition, ITransformationExpression thenExpression, ITransformationExpression elseExpression)
    {
        return new ConditionalExpression
        {
            Condition = condition,
            ThenExpression = thenExpression,
            ElseExpression = elseExpression
        };
    }

    /// <summary>
    /// Creates a collection projection expression.
    /// </summary>
    /// <param name="sourceCollection">The source collection expression.</param>
    /// <param name="itemAlias">The item alias.</param>
    /// <param name="body">The body expression.</param>
    /// <returns>The collection projection expression.</returns>
    public ICollectionProjectionExpression Project(ITransformationExpression sourceCollection, string itemAlias, ITransformationExpression body)
    {
        Guard.NotBlank(itemAlias, nameof(itemAlias));

        return new CollectionProjectionExpression
        {
            SourceExpression = sourceCollection,
            ItemAlias = itemAlias,
            BodyExpression = body
        };
    }

    /// <summary>
    /// Creates a map-shaped expression.
    /// </summary>
    /// <param name="properties">The property expressions.</param>
    /// <returns>The map-shaped expression.</returns>
    public IObjectExpression Object(IReadOnlyCollection<IObjectPropertyExpression> properties)
    {
        return new ObjectExpression
        {
            Properties = [.. properties]
        };
    }

    /// <summary>
    /// Creates a property expression.
    /// </summary>
    /// <param name="name">The property name.</param>
    /// <param name="expression">The property value expression.</param>
    /// <returns>The property expression.</returns>
    public IObjectPropertyExpression Property(string name, ITransformationExpression expression)
    {
        Guard.NotBlank(name, nameof(name));

        return new ObjectPropertyExpression
        {
            Name = name,
            Expression = expression
        };
    }

    /// <summary>
    /// Creates an ordered expression.
    /// </summary>
    /// <param name="items">The item expressions.</param>
    /// <returns>The ordered expression.</returns>
    public IArrayExpression Array(IReadOnlyCollection<ITransformationExpression> items)
    {
        return new ArrayExpression
        {
            Items = [.. items]
        };
    }
}
