namespace ButterMorph.Semantics;

using ButterMorph.Abstractions;
using ButterMorph.Core;

/// <summary>
/// Analyzes transformation documents without executing runtime behavior.
/// </summary>
public sealed class TransformationSemanticAnalyzer : ITransformationSemanticAnalyzer
{
    // Resolves schema paths used by mappings and rules.
    private readonly ISchemaPathResolver _schemaPathResolver;

    // Provides function descriptors for expression analysis.
    private readonly IFunctionRegistry _functionRegistry;

    // Provides validation rule descriptors for rule analysis.
    private readonly IValidationRuleRegistry _validationRuleRegistry;

    /// <summary>
    /// Initializes a new instance of the <see cref="TransformationSemanticAnalyzer"/> class.
    /// </summary>
    /// <param name="schemaPathResolver">The schema path resolver.</param>
    /// <param name="functionRegistry">The function registry.</param>
    /// <param name="validationRuleRegistry">The validation rule registry.</param>
    public TransformationSemanticAnalyzer(ISchemaPathResolver schemaPathResolver, IFunctionRegistry functionRegistry, IValidationRuleRegistry validationRuleRegistry)
    {
        _schemaPathResolver = schemaPathResolver;
        _functionRegistry = functionRegistry;
        _validationRuleRegistry = validationRuleRegistry;
    }

    /// <summary>
    /// Analyzes a transformation document.
    /// </summary>
    /// <param name="document">The transformation document.</param>
    /// <returns>The semantic analysis result.</returns>
    public SemanticAnalysisResult Analyze(ITransformationDocument document)
    {
        List<DiagnosticEntry> diagnostics = [];

        foreach (ITransformationMapping mapping in document.Mappings)
        {
            AnalyzeExpression(document, mapping.SourceExpression, new Dictionary<string, ISchemaNode>(StringComparer.Ordinal), diagnostics, mapping.TargetPath);
            TryResolveTargetPath(document, mapping.TargetPath, diagnostics);
        }

        foreach (IValidationRule rule in document.Validations)
        {
            AnalyzeValidationRule(document, rule, diagnostics);
        }

        return new SemanticAnalysisResult
        {
            Succeeded = diagnostics.Count == 0,
            Diagnostics = diagnostics
        };
    }

    // Analyzes one validation rule and its typed arguments.
    private void AnalyzeValidationRule(ITransformationDocument document, IValidationRule rule, List<DiagnosticEntry> diagnostics)
    {
        TryResolveTargetPath(document, rule.Path, diagnostics);

        IValidationRuleDescriptor descriptor;

        try
        {
            descriptor = _validationRuleRegistry.ResolveDescriptor(rule.RuleKey);
        }
        catch (KeyNotFoundException exception)
        {
            diagnostics.Add(CreateDiagnostic("BMSM007", exception.Message, rule.Path));
            return;
        }

        ValidateValidationArguments(document, rule, descriptor, diagnostics);
    }

    // Validates validation rule argument count and kinds.
    private void ValidateValidationArguments(ITransformationDocument document, IValidationRule rule, IValidationRuleDescriptor descriptor, List<DiagnosticEntry> diagnostics)
    {
        if (!HasValidCount(rule.Arguments.Count, descriptor.Parameters.Count, CountRequiredValidationParameters(descriptor)))
        {
            diagnostics.Add(CreateDiagnostic("BMSM008", $"Validation rule '{rule.RuleKey}' received an invalid argument count.", rule.Path));
            return;
        }

        int index = 0;

        foreach (ITransformationExpression argument in rule.Arguments)
        {
            IValidationRuleParameterDescriptor parameter = descriptor.Parameters.ElementAt(index);
            ExpressionSemanticShape shape = AnalyzeExpression(document, argument, new Dictionary<string, ISchemaNode>(StringComparer.Ordinal), diagnostics, rule.Path);

            if (shape.ValueKind != parameter.ValueKind)
            {
                diagnostics.Add(CreateDiagnostic("BMSM009", $"Validation rule '{rule.RuleKey}' argument '{parameter.Key}' has an invalid value kind.", rule.Path));
            }

            index++;
        }
    }

    // Analyzes an expression and infers its value kind.
    private ExpressionSemanticShape AnalyzeExpression(ITransformationDocument document, ITransformationExpression expression, Dictionary<string, ISchemaNode> aliases, List<DiagnosticEntry> diagnostics, string path)
    {
        if (expression is IPathExpression pathExpression)
        {
            return AnalyzePath(document, pathExpression.Path, aliases, diagnostics, path);
        }

        if (expression is IScalarLiteralExpression)
        {
            return CreateShape(FunctionValueKind.Scalar);
        }

        if (expression is IScalarCollectionLiteralExpression)
        {
            return CreateShape(FunctionValueKind.ScalarCollection);
        }

        if (expression is IFunctionCallExpression functionCallExpression)
        {
            return AnalyzeFunctionCall(document, functionCallExpression, aliases, diagnostics, path);
        }

        if (expression is IConditionalExpression conditionalExpression)
        {
            return AnalyzeConditional(document, conditionalExpression, aliases, diagnostics, path);
        }

        if (expression is ICollectionProjectionExpression projectionExpression)
        {
            return AnalyzeProjection(document, projectionExpression, aliases, diagnostics, path);
        }

        if (expression is IObjectExpression mapExpression)
        {
            foreach (IObjectPropertyExpression property in mapExpression.Properties)
            {
                AnalyzeExpression(document, property.Expression, aliases, diagnostics, path);
            }

            return CreateShape(FunctionValueKind.StructureNode);
        }

        if (expression is IArrayExpression arrayExpression)
        {
            foreach (ITransformationExpression item in arrayExpression.Items)
            {
                AnalyzeExpression(document, item, aliases, diagnostics, path);
            }

            return CreateShape(FunctionValueKind.StructureNode);
        }

        return CreateShape(FunctionValueKind.StructureNode);
    }

    // Analyzes a path expression against source schemas or alias schemas.
    private ExpressionSemanticShape AnalyzePath(ITransformationDocument document, string expressionPath, Dictionary<string, ISchemaNode> aliases, List<DiagnosticEntry> diagnostics, string diagnosticPath)
    {
        if (expressionPath.StartsWith("$", StringComparison.Ordinal))
        {
            return AnalyzeAbsolutePath(document, expressionPath, diagnostics, diagnosticPath);
        }

        string alias = expressionPath;
        string remainder = string.Empty;
        int separatorIndex = expressionPath.IndexOf('.', StringComparison.Ordinal);

        if (separatorIndex >= 0)
        {
            alias = expressionPath[..separatorIndex];
            remainder = expressionPath[(separatorIndex + 1)..];
        }

        if (!aliases.TryGetValue(alias, out ISchemaNode aliasNode))
        {
            diagnostics.Add(CreateDiagnostic("BMSM011", $"Alias '{alias}' is not available.", diagnosticPath));
            return CreateShape(FunctionValueKind.StructureNode);
        }

        if (string.IsNullOrWhiteSpace(remainder))
        {
            return CreateShape(aliasNode);
        }

        try
        {
            return CreateShape(_schemaPathResolver.Resolve(aliasNode, remainder));
        }
        catch (Exception exception) when (exception is FormatException || exception is KeyNotFoundException || exception is InvalidOperationException)
        {
            diagnostics.Add(CreateDiagnostic("BMSM002", exception.Message, diagnosticPath));
            return CreateShape(FunctionValueKind.StructureNode);
        }
    }

    // Analyzes an absolute source schema path.
    private ExpressionSemanticShape AnalyzeAbsolutePath(ITransformationDocument document, string expressionPath, List<DiagnosticEntry> diagnostics, string diagnosticPath)
    {
        string sourceKey = expressionPath[1..];
        string remainder = string.Empty;
        int separatorIndex = sourceKey.IndexOf('.', StringComparison.Ordinal);

        if (separatorIndex >= 0)
        {
            remainder = sourceKey[(separatorIndex + 1)..];
            sourceKey = sourceKey[..separatorIndex];
        }

        if (!document.SourceSchemas.TryGetValue(sourceKey, out IStructureSchema schema))
        {
            diagnostics.Add(CreateDiagnostic("BMSM001", $"Source schema '{sourceKey}' was not found.", diagnosticPath));
            return CreateShape(FunctionValueKind.StructureNode);
        }

        try
        {
            if (string.IsNullOrWhiteSpace(remainder))
            {
                return CreateShape(schema.Root);
            }

            return CreateShape(_schemaPathResolver.Resolve(schema.Root, remainder));
        }
        catch (Exception exception) when (exception is FormatException || exception is KeyNotFoundException || exception is InvalidOperationException)
        {
            diagnostics.Add(CreateDiagnostic("BMSM002", exception.Message, diagnosticPath));
            return CreateShape(FunctionValueKind.StructureNode);
        }
    }

    // Analyzes a function call against registered descriptors.
    private ExpressionSemanticShape AnalyzeFunctionCall(ITransformationDocument document, IFunctionCallExpression expression, Dictionary<string, ISchemaNode> aliases, List<DiagnosticEntry> diagnostics, string path)
    {
        IFunctionDescriptor descriptor;

        try
        {
            descriptor = _functionRegistry.ResolveDescriptor(expression.FunctionKey);
        }
        catch (KeyNotFoundException exception)
        {
            diagnostics.Add(CreateDiagnostic("BMSM004", exception.Message, path));
            return CreateShape(FunctionValueKind.StructureNode);
        }

        if (!HasValidCount(expression.Arguments.Count, descriptor.Parameters.Count, CountRequiredFunctionParameters(descriptor)))
        {
            diagnostics.Add(CreateDiagnostic("BMSM005", $"Function '{expression.FunctionKey}' received an invalid argument count.", path));
            return CreateShape(descriptor.ValueKind);
        }

        int index = 0;

        foreach (ITransformationExpression argument in expression.Arguments)
        {
            IFunctionParameterDescriptor parameter = descriptor.Parameters.ElementAt(index);
            ExpressionSemanticShape argumentShape = AnalyzeExpression(document, argument, aliases, diagnostics, path);

            if (argumentShape.ValueKind != parameter.ValueKind)
            {
                diagnostics.Add(CreateDiagnostic("BMSM006", $"Function '{expression.FunctionKey}' argument '{parameter.Key}' has an invalid value kind.", path));
            }

            index++;
        }

        return CreateShape(descriptor.ValueKind);
    }

    // Analyzes branch compatibility for a conditional expression.
    private ExpressionSemanticShape AnalyzeConditional(ITransformationDocument document, IConditionalExpression expression, Dictionary<string, ISchemaNode> aliases, List<DiagnosticEntry> diagnostics, string path)
    {
        AnalyzeExpression(document, expression.Condition, aliases, diagnostics, path);
        ExpressionSemanticShape thenShape = AnalyzeExpression(document, expression.ThenExpression, aliases, diagnostics, path);
        ExpressionSemanticShape elseShape = AnalyzeExpression(document, expression.ElseExpression, aliases, diagnostics, path);

        if (thenShape.ValueKind != elseShape.ValueKind)
        {
            diagnostics.Add(CreateDiagnostic("BMSM012", "Conditional branches produce incompatible value kinds.", path));
        }

        return thenShape;
    }

    // Analyzes projection source and body using alias scope.
    private ExpressionSemanticShape AnalyzeProjection(ITransformationDocument document, ICollectionProjectionExpression expression, Dictionary<string, ISchemaNode> aliases, List<DiagnosticEntry> diagnostics, string path)
    {
        ExpressionSemanticShape sourceShape = AnalyzeExpression(document, expression.SourceExpression, aliases, diagnostics, path);

        if (sourceShape.ValueKind != FunctionValueKind.StructureNodeCollection || !sourceShape.HasSchema)
        {
            diagnostics.Add(CreateDiagnostic("BMSM010", "Projection source is not collection-shaped.", path));
            return CreateShape(FunctionValueKind.StructureNodeCollection);
        }

        Dictionary<string, ISchemaNode> scopedAliases = new(aliases, StringComparer.Ordinal)
        {
            [expression.ItemAlias] = sourceShape.SchemaNode
        };
        AnalyzeExpression(document, expression.BodyExpression, scopedAliases, diagnostics, path);

        return CreateShape(FunctionValueKind.StructureNodeCollection);
    }

    // Resolves a target schema path and records target diagnostics.
    private void TryResolveTargetPath(ITransformationDocument document, string targetPath, List<DiagnosticEntry> diagnostics)
    {
        try
        {
            _schemaPathResolver.Resolve(document.TargetSchema.Root, targetPath);
        }
        catch (Exception exception) when (exception is FormatException || exception is KeyNotFoundException || exception is InvalidOperationException)
        {
            diagnostics.Add(CreateDiagnostic("BMSM003", exception.Message, targetPath));
        }
    }

    // Counts required function parameters.
    private static int CountRequiredFunctionParameters(IFunctionDescriptor descriptor)
    {
        int count = 0;

        foreach (IFunctionParameterDescriptor parameter in descriptor.Parameters)
        {
            if (parameter.IsRequired)
            {
                count++;
            }
        }

        return count;
    }

    // Counts required validation rule parameters.
    private static int CountRequiredValidationParameters(IValidationRuleDescriptor descriptor)
    {
        int count = 0;

        foreach (IValidationRuleParameterDescriptor parameter in descriptor.Parameters)
        {
            if (parameter.IsRequired)
            {
                count++;
            }
        }

        return count;
    }

    // Checks parameter count bounds.
    private static bool HasValidCount(int actual, int maximum, int minimum)
    {
        return actual >= minimum && actual <= maximum;
    }

    // Creates a shape from schema metadata.
    private static ExpressionSemanticShape CreateShape(ISchemaNode node)
    {
        FunctionValueKind valueKind = FunctionValueKind.StructureNode;
        ISchemaNode schemaNode = node;

        if (node.Kind == SchemaNodeKind.Scalar)
        {
            valueKind = FunctionValueKind.Scalar;
        }
        else if (node.Kind == SchemaNodeKind.Array)
        {
            valueKind = FunctionValueKind.StructureNodeCollection;
            schemaNode = ResolveArrayItemShape(node);
        }

        return new ExpressionSemanticShape
        {
            ValueKind = valueKind,
            HasSchema = true,
            SchemaNode = schemaNode
        };
    }

    // Creates a shape without schema metadata.
    private static ExpressionSemanticShape CreateShape(FunctionValueKind valueKind)
    {
        return new ExpressionSemanticShape
        {
            ValueKind = valueKind,
            HasSchema = false
        };
    }

    // Resolves array item shape without reporting diagnostics.
    private static ISchemaNode ResolveArrayItemShape(ISchemaNode arrayNode)
    {
        foreach (ISchemaNode child in arrayNode.Children)
        {
            if (string.Equals(child.Name, "$item", StringComparison.Ordinal))
            {
                return child;
            }
        }

        foreach (ISchemaNode child in arrayNode.Children)
        {
            return child;
        }

        return arrayNode;
    }

    // Creates a semantic diagnostic.
    private static DiagnosticEntry CreateDiagnostic(string code, string message, string path)
    {
        return new DiagnosticEntry
        {
            Code = code,
            Message = message,
            Path = path,
            Severity = "Error"
        };
    }
}
