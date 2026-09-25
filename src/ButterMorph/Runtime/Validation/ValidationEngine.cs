namespace ButterMorph.Validation;

using System;
using System.Collections.Generic;
using ButterMorph.Abstractions;
using ButterMorph.Core;

/// <summary>
/// Executes validation assertions and schemas against internal structure graphs.
/// </summary>
public sealed class ValidationEngine : IValidationEngine
{
    // Validates payloads against schemas when requested.
    private readonly ISchemaValidator _schemaValidator;

    // Evaluates boolean validation assertions.
    private readonly ITransformationExpressionEvaluator _expressionEvaluator;

    // Creates execution contexts for assertion evaluation.
    private readonly IExecutionContextFactory _executionContextFactory;

    /// <summary>
    /// Initializes a new instance of the <see cref="ValidationEngine"/> class.
    /// </summary>
    /// <param name="pathResolver">Reserved for constructor compatibility with existing service registrations.</param>
    /// <param name="ruleRegistry">Reserved for constructor compatibility with existing service registrations.</param>
    public ValidationEngine(IPathResolver pathResolver, IValidationRuleRegistry ruleRegistry)
        : this(pathResolver, ruleRegistry, null, null, null)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ValidationEngine"/> class.
    /// </summary>
    /// <param name="pathResolver">Reserved for constructor compatibility with existing service registrations.</param>
    /// <param name="ruleRegistry">Reserved for constructor compatibility with existing service registrations.</param>
    /// <param name="schemaValidator">The schema validator.</param>
    /// <param name="expressionEvaluator">The expression evaluator.</param>
    /// <param name="executionContextFactory">The execution context factory.</param>
    public ValidationEngine(
        IPathResolver pathResolver,
        IValidationRuleRegistry ruleRegistry,
        ISchemaValidator schemaValidator,
        ITransformationExpressionEvaluator expressionEvaluator,
        IExecutionContextFactory executionContextFactory)
    {
        _schemaValidator = schemaValidator;
        _expressionEvaluator = expressionEvaluator;
        _executionContextFactory = executionContextFactory;
    }

    /// <summary>
    /// Executes a validation request.
    /// </summary>
    /// <param name="request">The validation request.</param>
    /// <returns>The validation result.</returns>
    public ValidationResult Validate(ValidationRequest request)
    {
        List<DiagnosticEntry> diagnostics = [];
        ValidationScope scope = ResolveScope(request);

        if (!scope.HasValidationDocument && request.Schema == null)
        {
            diagnostics.Add(CreateDiagnostic("BMVL001", "Validation request must include a schema or an IValidationDocument definition.", string.Empty));
            return CreateResult(diagnostics);
        }

        string payloadAlias = ResolvePayloadAlias(request.PayloadAlias, string.Empty);
        IStructureGraph graph = ResolveGraph(request, payloadAlias);

        if (request.Schema != null)
        {
            if (graph == null)
            {
                diagnostics.Add(CreateDiagnostic("BMVL006", "Validation payload graph is required.", string.Empty));
                return CreateResult(diagnostics);
            }

            ValidateSchema(request, graph, request.Schema, payloadAlias, diagnostics);
        }

        if (!scope.HasValidationDocument)
        {
            return CreateResult(diagnostics);
        }

        ValidateStatements(graph, scope, request, payloadAlias, diagnostics);

        return CreateResult(diagnostics);
    }

    private void ValidateSchema(
        ValidationRequest request,
        IStructureGraph graph,
        IStructureSchema schema,
        string payloadAlias,
        List<DiagnosticEntry> diagnostics)
    {
        if (_schemaValidator == null)
        {
            diagnostics.Add(CreateDiagnostic("BMVL008", "A schema validator must be registered before validating schemas.", string.Empty));
            return;
        }

        ValidationResult schemaResult = _schemaValidator.Validate(new ValidationRequest
        {
            SourceGraph = graph,
            Sources = request.Sources,
            PayloadAlias = payloadAlias,
            Schema = schema,
            Schemas = request.Schemas,
            Definition = request.Definition
        });

        diagnostics.AddRange(schemaResult.Diagnostics);
    }

    private void ValidateStatements(
        IStructureGraph graph,
        ValidationScope scope,
        ValidationRequest request,
        string payloadAlias,
        List<DiagnosticEntry> diagnostics)
    {
        if (scope.Statements.Count == 0)
        {
            return;
        }

        if (_expressionEvaluator == null || _executionContextFactory == null)
        {
            diagnostics.Add(CreateDiagnostic("BMVL004", "An expression evaluator must be registered before validating assertions.", string.Empty));
            return;
        }

        Dictionary<string, IStructureGraph> sources = new(request.Sources, StringComparer.Ordinal);
        if (graph != null)
        {
            sources[payloadAlias] = graph;
        }

        IExecutionContext executionContext = _executionContextFactory.Create(sources);
        Dictionary<string, IStructureNode> aliases = new(StringComparer.Ordinal);
        Dictionary<string, string> aliasPaths = new(StringComparer.Ordinal);

        foreach (KeyValuePair<string, IStructureGraph> source in sources)
        {
            aliases[source.Key] = source.Value.Root;
            aliasPaths[source.Key] = "$" + source.Key;
        }

        foreach (IValidationStatement statement in scope.Statements)
        {
            ValidateStatement(statement, executionContext, aliases, aliasPaths, payloadAlias, diagnostics);
        }
    }

    private void ValidateStatement(
        IValidationStatement statement,
        IExecutionContext executionContext,
        IReadOnlyDictionary<string, IStructureNode> aliases,
        IReadOnlyDictionary<string, string> aliasPaths,
        string payloadAlias,
        List<DiagnosticEntry> diagnostics)
    {
        if (statement is IValidationAssertion assertion)
        {
            ValidateAssertion(assertion, executionContext, aliases, aliasPaths, payloadAlias, diagnostics);
            return;
        }

        if (statement is IValidationForEach forEach)
        {
            ValidateForEach(forEach, executionContext, aliases, aliasPaths, payloadAlias, diagnostics);
            return;
        }

        diagnostics.Add(CreateDiagnostic("BMVL009", $"Validation statement '{statement.GetType().Name}' is not supported.", string.Empty));
    }

    private void ValidateAssertion(
        IValidationAssertion assertion,
        IExecutionContext executionContext,
        IReadOnlyDictionary<string, IStructureNode> aliases,
        IReadOnlyDictionary<string, string> aliasPaths,
        string payloadAlias,
        List<DiagnosticEntry> diagnostics)
    {
        ITransformationExpressionEvaluationResult result;

        try
        {
            result = _expressionEvaluator.Evaluate(new TransformationExpressionEvaluationContext
            {
                ExecutionContext = executionContext,
                Expression = assertion.Expression,
                Aliases = aliases
            });
        }
        catch (Exception exception) when (exception is FormatException || exception is KeyNotFoundException || exception is InvalidOperationException || exception is IndexOutOfRangeException)
        {
            diagnostics.Add(CreateDiagnostic("BMVL005", exception.Message, NormalizeAssertionPath(assertion.Path, payloadAlias, aliasPaths)));
            return;
        }

        if (!result.Succeeded)
        {
            diagnostics.Add(CreateDiagnostic("BMVL005", "Validation assertion could not be evaluated.", NormalizeAssertionPath(assertion.Path, payloadAlias, aliasPaths)));
            diagnostics.AddRange(result.Diagnostics);
            return;
        }

        if (!IsTruthyBoolean(result.Result))
        {
            diagnostics.Add(CreateDiagnostic("BMVL004", assertion.Message, NormalizeAssertionPath(assertion.Path, payloadAlias, aliasPaths)));
        }
    }

    private void ValidateForEach(
        IValidationForEach forEach,
        IExecutionContext executionContext,
        IReadOnlyDictionary<string, IStructureNode> aliases,
        IReadOnlyDictionary<string, string> aliasPaths,
        string payloadAlias,
        List<DiagnosticEntry> diagnostics)
    {
        ITransformationExpressionEvaluationResult sourceResult;

        try
        {
            sourceResult = _expressionEvaluator.Evaluate(new TransformationExpressionEvaluationContext
            {
                ExecutionContext = executionContext,
                Expression = forEach.SourceExpression,
                Aliases = aliases
            });
        }
        catch (Exception exception) when (exception is FormatException || exception is KeyNotFoundException || exception is InvalidOperationException || exception is IndexOutOfRangeException)
        {
            diagnostics.Add(CreateDiagnostic("BMVL005", exception.Message, ResolveForEachPath(forEach, payloadAlias, aliasPaths)));
            return;
        }

        if (!sourceResult.Succeeded)
        {
            diagnostics.Add(CreateDiagnostic("BMVL005", "Validation foreach source could not be evaluated.", ResolveForEachPath(forEach, payloadAlias, aliasPaths)));
            diagnostics.AddRange(sourceResult.Diagnostics);
            return;
        }

        if (sourceResult.Result is IStructureNodeCollectionFunctionResult nodeCollection)
        {
            ValidateForEachNodes(forEach, executionContext, aliases, aliasPaths, payloadAlias, nodeCollection.Nodes, diagnostics);
            return;
        }

        if (sourceResult.Result is IScalarCollectionFunctionResult scalarCollection)
        {
            List<IStructureNode> scalarNodes = [];
            int scalarIndex = 0;

            foreach (IScalarValue value in scalarCollection.Values)
            {
                scalarNodes.Add(new ScalarStructureNode
                {
                    Name = scalarIndex.ToString(System.Globalization.CultureInfo.InvariantCulture),
                    Value = value
                });
                scalarIndex++;
            }

            ValidateForEachNodes(forEach, executionContext, aliases, aliasPaths, payloadAlias, scalarNodes, diagnostics);
            return;
        }

        diagnostics.Add(CreateDiagnostic("BMVL009", "Validation foreach source must evaluate to a collection.", ResolveForEachPath(forEach, payloadAlias, aliasPaths)));
    }

    private void ValidateForEachNodes(
        IValidationForEach forEach,
        IExecutionContext executionContext,
        IReadOnlyDictionary<string, IStructureNode> aliases,
        IReadOnlyDictionary<string, string> aliasPaths,
        string payloadAlias,
        IReadOnlyCollection<IStructureNode> nodes,
        List<DiagnosticEntry> diagnostics)
    {
        string alias = ResolvePayloadAlias(forEach.ItemAlias, "item");
        int index = 0;

        foreach (IStructureNode node in nodes)
        {
            Dictionary<string, IStructureNode> childAliases = new(aliases, StringComparer.Ordinal)
            {
                [alias] = node
            };
            Dictionary<string, string> childAliasPaths = new(aliasPaths, StringComparer.Ordinal)
            {
                [alias] = CreateIndexedForEachPath(forEach, index, aliasPaths)
            };

            foreach (IValidationStatement statement in forEach.Statements)
            {
                ValidateStatement(statement, executionContext, childAliases, childAliasPaths, payloadAlias, diagnostics);
            }

            index++;
        }
    }

    // Creates a validation result from accumulated diagnostics.
    private static ValidationResult CreateResult(IReadOnlyCollection<DiagnosticEntry> diagnostics)
    {
        return new ValidationResult
        {
            IsValid = diagnostics.Count == 0,
            Diagnostics = diagnostics
        };
    }

    private static ValidationScope ResolveScope(ValidationRequest request)
    {
        if (request.Definition is IValidationDocument validationDocument)
        {
            return new ValidationScope
            {
                HasValidationDocument = true,
                Statements = validationDocument.Statements
            };
        }

        return new ValidationScope
        {
            HasValidationDocument = false,
            Statements = []
        };
    }

    private static IStructureGraph ResolveGraph(ValidationRequest request, string payloadAlias)
    {
        if (request.SourceGraph != null)
        {
            return request.SourceGraph;
        }

        if (request.Sources.TryGetValue(payloadAlias, out IStructureGraph graph))
        {
            return graph;
        }

        return null;
    }

    private static bool IsTruthyBoolean(IFunctionResult result)
    {
        if (result is not IScalarFunctionResult scalarResult || scalarResult.Value.IsNull)
        {
            return false;
        }

        return string.Equals(scalarResult.Value.RawValue, "true", StringComparison.OrdinalIgnoreCase);
    }

    private static string ResolvePayloadAlias(string preferredAlias, string fallbackAlias)
    {
        string alias = preferredAlias;

        if (string.IsNullOrWhiteSpace(alias))
        {
            alias = fallbackAlias;
        }

        if (string.IsNullOrWhiteSpace(alias))
        {
            alias = "source";
        }

        return alias.TrimStart('$');
    }

    private static string NormalizeAssertionPath(string path, string payloadAlias, IReadOnlyDictionary<string, string> aliasPaths)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return string.Empty;
        }

        path = ResolveAliasPath(path, aliasPaths);

        string prefix = "$" + payloadAlias + ".";

        if (path.StartsWith(prefix, StringComparison.Ordinal))
        {
            return path[prefix.Length..];
        }

        if (string.Equals(path, "$" + payloadAlias, StringComparison.Ordinal))
        {
            return "$root";
        }

        return path;
    }

    private static string ResolveAliasPath(string path, IReadOnlyDictionary<string, string> aliasPaths)
    {
        if (!path.StartsWith("$", StringComparison.Ordinal))
        {
            return path;
        }

        string withoutPrefix = path[1..];
        int endIndex = withoutPrefix.Length;
        int dotIndex = withoutPrefix.IndexOf('.', StringComparison.Ordinal);
        int bracketIndex = withoutPrefix.IndexOf('[', StringComparison.Ordinal);

        if (dotIndex >= 0)
        {
            endIndex = Math.Min(endIndex, dotIndex);
        }

        if (bracketIndex >= 0)
        {
            endIndex = Math.Min(endIndex, bracketIndex);
        }

        string alias = withoutPrefix[..endIndex];

        if (!aliasPaths.TryGetValue(alias, out string aliasPath))
        {
            return path;
        }

        return aliasPath + withoutPrefix[endIndex..];
    }

    private static string ResolveForEachPath(IValidationForEach forEach, string payloadAlias, IReadOnlyDictionary<string, string> aliasPaths)
    {
        if (forEach.SourceExpression is IPathExpression pathExpression)
        {
            return NormalizeAssertionPath(pathExpression.Path, payloadAlias, aliasPaths);
        }

        return string.Empty;
    }

    private static string CreateIndexedForEachPath(IValidationForEach forEach, int index, IReadOnlyDictionary<string, string> aliasPaths)
    {
        if (forEach.SourceExpression is IPathExpression pathExpression)
        {
            return ResolveAliasPath(pathExpression.Path, aliasPaths) + "[" + index.ToString(System.Globalization.CultureInfo.InvariantCulture) + "]";
        }

        return "$" + ResolvePayloadAlias(forEach.ItemAlias, "item") + "[" + index.ToString(System.Globalization.CultureInfo.InvariantCulture) + "]";
    }

    // Creates an error diagnostic for validation orchestration failures.
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

    private sealed class ValidationScope
    {
        internal bool HasValidationDocument { get; set; }

        internal IReadOnlyCollection<IValidationStatement> Statements { get; set; } = [];
    }
}
