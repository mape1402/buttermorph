namespace ButterMorph.Dsl;

using ButterMorph.Abstractions;
using ButterMorph.Core;
using ButterMorph.Modeling;

// Builds typed ButterMorph documents from internal DSL syntax nodes.
internal sealed class AstBuilder
{
    // Carries the original DSL definition into the parsed document.
    private readonly IDslDefinition _definition;

    // Creates expression containers through the public modeling API.
    private readonly IExpressionBuilder _expressions = ButterMorphModel.Expressions;

    internal AstBuilder(IDslDefinition definition)
    {
        _definition = definition;
    }

    internal ITransformationDocument Build(DocumentNode node)
    {
        ITransformationDocumentBuilder builder = ButterMorphModel.CreateDocument()
            .WithDefinition(_definition);

        foreach (KeyValuePair<string, string> metadata in node.Metadata)
        {
            builder.WithMetadata(metadata.Key, metadata.Value);
        }

        foreach (AssignmentNode assignment in node.Assignments)
        {
            builder.Map(BuildExpression(assignment.Expression), assignment.TargetPath);
        }

        foreach (ValidationNode validation in node.Validations)
        {
            builder.WithValidation(new ValidationRule
            {
                Path = validation.Path,
                RuleKey = validation.RuleKey,
                Arguments = BuildExpressions(validation.Arguments)
            });
        }

        return builder.Build();
    }

    private ITransformationExpression BuildExpression(AstNode node)
    {
        if (node is PathNode path)
        {
            return _expressions.Path(path.Path);
        }

        if (node is LiteralNode literal)
        {
            if (literal.IsNull)
            {
                return _expressions.NullScalar();
            }

            if (string.Equals(literal.DataType, "Boolean", StringComparison.Ordinal))
            {
                return _expressions.Boolean(string.Equals(literal.RawValue, "true", StringComparison.Ordinal));
            }

            if (string.Equals(literal.DataType, "Number", StringComparison.Ordinal))
            {
                return _expressions.Number(literal.RawValue);
            }

            return _expressions.Scalar(literal.DataType, literal.RawValue);
        }

        if (node is FunctionCallNode function)
        {
            return _expressions.Function(function.FunctionKey, BuildExpressions(function.Arguments));
        }

        if (node is ConditionNode condition)
        {
            return _expressions.When(
                BuildExpression(condition.Condition),
                BuildExpression(condition.ThenExpression),
                BuildExpression(condition.ElseExpression));
        }

        if (node is ProjectionNode projection)
        {
            return _expressions.Project(
                BuildExpression(projection.SourceExpression),
                projection.ItemAlias,
                BuildExpression(projection.BodyExpression));
        }

        if (node is MapExpressionNode map)
        {
            List<IObjectPropertyExpression> properties = [];

            foreach (PropertyExpressionNode property in map.Properties)
            {
                properties.Add(_expressions.Property(property.Name, BuildExpression(property.Expression)));
            }

            return _expressions.Object(properties);
        }

        if (node is OrderedExpressionNode ordered)
        {
            return _expressions.Array(BuildExpressions(ordered.Items));
        }

        throw new InvalidOperationException($"Unsupported DSL node '{node.GetType().Name}'.");
    }

    private IReadOnlyCollection<ITransformationExpression> BuildExpressions(IReadOnlyCollection<AstNode> nodes)
    {
        List<ITransformationExpression> expressions = [];

        foreach (AstNode node in nodes)
        {
            expressions.Add(BuildExpression(node));
        }

        return expressions;
    }
}
