namespace ButterMorph.Dsl;

using System.Globalization;
using System.Text;
using ButterMorph.Abstractions;

/// <summary>
/// Exports validation documents into ButterMorph validation DSL text.
/// </summary>
public sealed class ValidationDslExporter : IValidationDslExporter
{
    /// <summary>
    /// Exports a validation document into DSL text.
    /// </summary>
    /// <param name="document">The validation document.</param>
    /// <returns>The exported DSL text.</returns>
    public string Export(IValidationDocument document)
    {
        StringBuilder builder = new();

        WriteStatements(builder, document);

        return builder.ToString().TrimEnd();
    }

    // Writes validation statements preserving document order.
    private static void WriteStatements(StringBuilder builder, IValidationDocument document)
    {
        if (document.Statements.Count == 0)
        {
            return;
        }

        builder.AppendLine("validate {");

        foreach (IValidationStatement statement in document.Statements)
        {
            WriteStatement(builder, statement, 1);
        }

        builder.AppendLine("}");
    }

    // Writes one validation statement.
    private static void WriteStatement(StringBuilder builder, IValidationStatement statement, int indent)
    {
        if (statement is IValidationAssertion assertion)
        {
            WriteIndent(builder, indent);
            builder.Append("assert ");
            builder.Append(WriteExpression(assertion.Expression));
            builder.Append(": ");
            builder.AppendLine(WriteString(assertion.Message));
            return;
        }

        if (statement is IValidationForEach forEach)
        {
            WriteIndent(builder, indent);
            builder.Append("foreach ");
            builder.Append(WriteExpression(forEach.SourceExpression));
            builder.Append(" as ");
            builder.Append(string.IsNullOrWhiteSpace(forEach.ItemAlias) ? "item" : forEach.ItemAlias);
            builder.AppendLine(" {");

            foreach (IValidationStatement childStatement in forEach.Statements)
            {
                WriteStatement(builder, childStatement, indent + 1);
            }

            WriteIndent(builder, indent);
            builder.AppendLine("}");
            return;
        }

        throw new InvalidOperationException($"Validation statement '{statement.GetType().Name}' cannot be exported.");
    }

    // Writes any supported transformation expression.
    private static string WriteExpression(ITransformationExpression expression)
    {
        if (expression is IPathExpression path)
        {
            return path.Path;
        }

        if (expression is IScalarLiteralExpression scalar)
        {
            return WriteScalar(scalar.Value);
        }

        if (expression is IScalarCollectionLiteralExpression scalarCollection)
        {
            return $"scalars({string.Join(", ", scalarCollection.Values.Select(WriteScalar))})";
        }

        if (expression is IFunctionCallExpression function)
        {
            return $"{function.FunctionKey}({string.Join(", ", function.Arguments.Select(WriteExpression))})";
        }

        if (expression is IConditionalExpression conditional)
        {
            return $"when({WriteExpression(conditional.Condition)}, {WriteExpression(conditional.ThenExpression)}, {WriteExpression(conditional.ElseExpression)})";
        }

        if (expression is ICollectionProjectionExpression projection)
        {
            return $"project {WriteExpression(projection.SourceExpression)} as {projection.ItemAlias} => {WriteExpression(projection.BodyExpression)}";
        }

        if (expression is IObjectExpression map)
        {
            return $"{{ {string.Join(", ", map.Properties.Select(WriteProperty))} }}";
        }

        if (expression is IArrayExpression ordered)
        {
            return $"[{string.Join(", ", ordered.Items.Select(WriteExpression))}]";
        }

        throw new InvalidOperationException($"Expression kind '{expression.Kind}' cannot be exported.");
    }

    // Writes one map-shaped expression property.
    private static string WriteProperty(IObjectPropertyExpression property)
    {
        return $"{property.Name}: {WriteExpression(property.Expression)}";
    }

    // Writes one scalar value.
    private static string WriteScalar(IScalarValue value)
    {
        if (value.IsNull)
        {
            return "null";
        }

        if (string.Equals(value.DataType, "Boolean", StringComparison.Ordinal))
        {
            return value.RawValue.ToLower(CultureInfo.InvariantCulture);
        }

        if (string.Equals(value.DataType, "Number", StringComparison.Ordinal))
        {
            return value.RawValue;
        }

        return WriteString(value.RawValue);
    }

    // Escapes a string literal deterministically.
    private static string WriteString(string value)
    {
        StringBuilder builder = new();
        builder.Append('"');

        foreach (char character in value)
        {
            if (character == '"')
            {
                builder.Append("\\\"");
            }
            else if (character == '\\')
            {
                builder.Append("\\\\");
            }
            else if (character == '\n')
            {
                builder.Append("\\n");
            }
            else if (character == '\r')
            {
                builder.Append("\\r");
            }
            else if (character == '\t')
            {
                builder.Append("\\t");
            }
            else
            {
                builder.Append(character);
            }
        }

        builder.Append('"');
        return builder.ToString();
    }

    // Writes indentation using two spaces per level.
    private static void WriteIndent(StringBuilder builder, int level)
    {
        for (int index = 0; index < level; index++)
        {
            builder.Append("  ");
        }
    }
}
