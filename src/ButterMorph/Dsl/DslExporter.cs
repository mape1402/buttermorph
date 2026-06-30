namespace ButterMorph.Dsl;

using System.Globalization;
using System.Text;
using ButterMorph.Abstractions;

/// <summary>
/// Exports transformation documents into ButterMorph DSL text.
/// </summary>
public sealed class DslExporter : IDslExporter
{
    /// <summary>
    /// Exports a transformation document into DSL text.
    /// </summary>
    /// <param name="document">The transformation document.</param>
    /// <returns>The exported DSL text.</returns>
    public string Export(ITransformationDocument document)
    {
        StringBuilder builder = new();

        WriteMetadata(builder, document);
        WriteTarget(builder, document);
        WriteValidations(builder, document);

        return builder.ToString().TrimEnd();
    }

    // Writes document metadata using deterministic key ordering.
    private static void WriteMetadata(StringBuilder builder, ITransformationDocument document)
    {
        if (document.Metadata.Count == 0)
        {
            return;
        }

        builder.AppendLine("metadata {");

        foreach (KeyValuePair<string, string> entry in document.Metadata.OrderBy(entry => entry.Key, StringComparer.Ordinal))
        {
            builder.Append("  ");
            builder.Append(WriteMetadataKey(entry.Key));
            builder.Append(": ");
            builder.AppendLine(WriteString(entry.Value));
        }

        builder.AppendLine("}");
        builder.AppendLine();
    }

    // Writes a metadata key as an identifier when possible, otherwise as a string literal.
    private static string WriteMetadataKey(string key)
    {
        if (IsIdentifier(key))
        {
            return key;
        }

        return WriteString(key);
    }

    // Writes transformation mappings as nested target blocks when possible.
    private static void WriteTarget(StringBuilder builder, ITransformationDocument document)
    {
        if (document.Mappings.Count == 0)
        {
            return;
        }

        DslTargetNode root = new()
        {
            Name = "$root"
        };
        List<ITransformationMapping> flatMappings = [];

        foreach (ITransformationMapping mapping in document.Mappings)
        {
            if (CanRenderNested(mapping.TargetPath))
            {
                AddNestedMapping(root, mapping);
            }
            else
            {
                flatMappings.Add(mapping);
            }
        }

        builder.AppendLine("target {");
        WriteTargetChildren(builder, root, 1);

        foreach (ITransformationMapping mapping in flatMappings)
        {
            WriteIndent(builder, 1);
            builder.Append(mapping.TargetPath);
            builder.Append(": ");
            builder.AppendLine(WriteExpression(mapping.SourceExpression));
        }

        builder.AppendLine("}");
        builder.AppendLine();
    }

    // Writes validation rules preserving document order.
    private static void WriteValidations(StringBuilder builder, ITransformationDocument document)
    {
        if (document.Validations.Count == 0)
        {
            return;
        }

        builder.AppendLine("validate {");

        foreach (IValidationRule rule in document.Validations)
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

    // Adds a dot-delimited target mapping to the render tree.
    private static void AddNestedMapping(DslTargetNode root, ITransformationMapping mapping)
    {
        DslTargetNode current = root;
        string[] segments = mapping.TargetPath.Split('.', StringSplitOptions.RemoveEmptyEntries);

        foreach (string segment in segments)
        {
            if (!current.Children.TryGetValue(segment, out DslTargetNode child))
            {
                child = new DslTargetNode
                {
                    Name = segment
                };
                current.Children[segment] = child;
            }

            current = child;
        }

        current.HasExpression = true;
        current.Expression = WriteExpression(mapping.SourceExpression);
    }

    // Writes target tree children in insertion order.
    private static void WriteTargetChildren(StringBuilder builder, DslTargetNode node, int level)
    {
        foreach (DslTargetNode child in node.Children.Values)
        {
            WriteIndent(builder, level);
            builder.Append(child.Name);

            if (child.Children.Count > 0)
            {
                builder.AppendLine(" {");
                WriteTargetChildren(builder, child, level + 1);
                WriteIndent(builder, level);
                builder.AppendLine("}");
                continue;
            }

            builder.Append(": ");
            builder.AppendLine(child.Expression);
        }
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

    // Detects whether a target path can be rendered as clean nested blocks.
    private static bool CanRenderNested(string targetPath)
    {
        string[] segments = targetPath.Split('.', StringSplitOptions.RemoveEmptyEntries);

        if (segments.Length == 0)
        {
            return false;
        }

        foreach (string segment in segments)
        {
            if (!IsIdentifier(segment))
            {
                return false;
            }
        }

        return true;
    }

    // Checks whether a DSL token can be written as an identifier.
    private static bool IsIdentifier(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        if (!char.IsLetter(value[0]) && value[0] != '_')
        {
            return false;
        }

        foreach (char character in value)
        {
            if (!char.IsLetterOrDigit(character) && character != '_')
            {
                return false;
            }
        }

        return true;
    }
}
