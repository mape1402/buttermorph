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

        WriteRules(builder, document);
        WriteAssertions(builder, document);

        return builder.ToString().TrimEnd();
    }

    // Writes validation rules preserving document order.
    private static void WriteRules(StringBuilder builder, IValidationDocument document)
    {
        if (document.Rules.Count == 0)
        {
            return;
        }

        builder.AppendLine("validate {");

        foreach (IValidationRule rule in document.Rules)
        {
            WriteIndent(builder, 1);
            builder.Append(rule.Path);
            builder.Append(": ");
            builder.Append(rule.RuleKey);

            if (rule.Arguments.Count > 0)
            {
                builder.Append('(');
                builder.Append(string.Join(", ", rule.Arguments.Select(WriteExpression)));
                builder.Append(')');
            }

            builder.AppendLine();
        }

        builder.AppendLine("}");
    }

    // Writes boolean validation assertions preserving document order.
    private static void WriteAssertions(StringBuilder builder, IValidationDocument document)
    {
        if (document.Assertions.Count == 0)
        {
            return;
        }

        if (document.Rules.Count > 0)
        {
            builder.AppendLine();
        }

        string payloadAlias = string.IsNullOrWhiteSpace(document.PayloadAlias)
            ? "source"
            : document.PayloadAlias;
        string schemaKey = string.IsNullOrWhiteSpace(document.SchemaKey)
            ? "Schema"
            : document.SchemaKey;

        builder.Append("validate $");
        builder.Append(payloadAlias.TrimStart('$'));
        builder.Append(" against ");
        builder.Append(schemaKey);
        builder.AppendLine(" {");

        foreach (IValidationAssertion assertion in document.Assertions)
        {
            WriteIndent(builder, 1);
            builder.Append("assert ");
            builder.Append(WriteExpression(assertion.Expression));
            builder.Append(": ");
            builder.AppendLine(WriteString(assertion.Message));
        }

        builder.AppendLine("}");
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
