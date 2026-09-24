namespace ButterMorph.Validation;

using System;
using System.Collections.Generic;
using ButterMorph.Abstractions;

/// <summary>
/// Executes validation rules and schemas against internal structure graphs.
/// </summary>
public sealed class ValidationEngine : IValidationEngine
{
    // Resolves validation paths from the graph root.
    private readonly IPathResolver _pathResolver;

    // Provides rule behavior registered by consumers or higher layers.
    private readonly IValidationRuleRegistry _ruleRegistry;

    // Validates payloads against schemas when requested.
    private readonly ISchemaValidator _schemaValidator;

    // Evaluates boolean validation assertions.
    private readonly ITransformationExpressionEvaluator _expressionEvaluator;

    // Creates execution contexts for assertion evaluation.
    private readonly IExecutionContextFactory _executionContextFactory;

    /// <summary>
    /// Initializes a new instance of the <see cref="ValidationEngine"/> class.
    /// </summary>
    /// <param name="pathResolver">The path resolver.</param>
    /// <param name="ruleRegistry">The validation rule registry.</param>
    public ValidationEngine(IPathResolver pathResolver, IValidationRuleRegistry ruleRegistry)
        : this(pathResolver, ruleRegistry, null, null, null)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ValidationEngine"/> class.
    /// </summary>
    /// <param name="pathResolver">The path resolver.</param>
    /// <param name="ruleRegistry">The validation rule registry.</param>
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
        if (pathResolver is null)
        {
            throw new InvalidOperationException("A path resolver must be registered before executing validations.");
        }

        if (ruleRegistry is null)
        {
            throw new InvalidOperationException("A validation rule registry must be registered before executing validations.");
        }

        _pathResolver = pathResolver;
        _ruleRegistry = ruleRegistry;
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
            diagnostics.Add(CreateDiagnostic("BMVL001", "Validation request definition must implement IValidationDocument.", string.Empty));
            return CreateResult(diagnostics);
        }

        IStructureGraph graph = ResolveGraph(request, scope.PayloadAlias);
        IStructureSchema schema = ResolveSchema(request, scope.SchemaKey);

        if (schema != null)
        {
            ValidateSchema(request, graph, schema, scope.PayloadAlias, diagnostics);
        }
        else if (!string.IsNullOrWhiteSpace(scope.SchemaKey))
        {
            diagnostics.Add(CreateDiagnostic("BMVL007", $"Validation schema '{scope.SchemaKey}' was not found.", scope.SchemaKey));
        }

        if (!scope.HasValidationDocument)
        {
            return CreateResult(diagnostics);
        }

        bool graphRequired = schema != null || scope.Rules.Count > 0;

        if (graph == null && graphRequired)
        {
            diagnostics.Add(CreateDiagnostic("BMVL006", "Validation payload graph is required.", string.Empty));
            return CreateResult(diagnostics);
        }

        foreach (IValidationRule rule in scope.Rules)
        {
            ValidateRule(graph.Root, rule, diagnostics);
        }

        ValidateAssertions(graph, scope, request, diagnostics);

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

    // Resolves one rule target and delegates behavior to the registered handler.
    private void ValidateRule(IStructureNode root, IValidationRule rule, List<DiagnosticEntry> diagnostics)
    {
        if (!TryResolvePath(root, rule, diagnostics, out IStructureNode node))
        {
            return;
        }

        IValidationRuleHandler handler;

        try
        {
            handler = _ruleRegistry.Resolve(rule.RuleKey);
        }
        catch (KeyNotFoundException exception)
        {
            diagnostics.Add(CreateDiagnostic("BMVL003", exception.Message, rule.Path));
            return;
        }

        ValidationRuleContext context = new()
        {
            Rule = rule,
            Node = node,
            Path = rule.Path
        };

        diagnostics.AddRange(handler.Validate(context));
    }

    private void ValidateAssertions(IStructureGraph graph, ValidationScope scope, ValidationRequest request, List<DiagnosticEntry> diagnostics)
    {
        if (scope.Assertions.Count == 0)
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
            sources[scope.PayloadAlias] = graph;
        }

        IExecutionContext executionContext = _executionContextFactory.Create(sources);
        Dictionary<string, IStructureNode> aliases = new(StringComparer.Ordinal);
        if (graph != null)
        {
            aliases[scope.PayloadAlias] = graph.Root;
        }

        foreach (IValidationAssertion assertion in scope.Assertions)
        {
            ValidateAssertion(assertion, executionContext, aliases, scope.PayloadAlias, diagnostics);
        }
    }

    private void ValidateAssertion(
        IValidationAssertion assertion,
        IExecutionContext executionContext,
        IReadOnlyDictionary<string, IStructureNode> aliases,
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
            diagnostics.Add(CreateDiagnostic("BMVL005", exception.Message, NormalizeAssertionPath(assertion.Path, payloadAlias)));
            return;
        }

        if (!result.Succeeded)
        {
            diagnostics.Add(CreateDiagnostic("BMVL005", "Validation assertion could not be evaluated.", NormalizeAssertionPath(assertion.Path, payloadAlias)));
            diagnostics.AddRange(result.Diagnostics);
            return;
        }

        if (!IsTruthyBoolean(result.Result))
        {
            diagnostics.Add(CreateDiagnostic("BMVL004", assertion.Message, NormalizeAssertionPath(assertion.Path, payloadAlias)));
        }
    }

    // Resolves the rule path and converts navigation failures into diagnostics.
    private bool TryResolvePath(IStructureNode root, IValidationRule rule, List<DiagnosticEntry> diagnostics, out IStructureNode node)
    {
        node = root;

        try
        {
            node = _pathResolver.Resolve(root, rule.Path);
            return true;
        }
        catch (Exception exception) when (exception is FormatException || exception is KeyNotFoundException || exception is InvalidOperationException || exception is IndexOutOfRangeException)
        {
            diagnostics.Add(CreateDiagnostic("BMVL002", exception.Message, rule.Path));
            return false;
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
                PayloadAlias = ResolvePayloadAlias(validationDocument.PayloadAlias, request.PayloadAlias),
                SchemaKey = validationDocument.SchemaKey,
                Rules = validationDocument.Rules,
                Assertions = validationDocument.Assertions
            };
        }

        return new ValidationScope
        {
            HasValidationDocument = false,
            PayloadAlias = ResolvePayloadAlias(request.PayloadAlias, string.Empty),
            SchemaKey = string.Empty,
            Rules = [],
            Assertions = []
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

    private static IStructureSchema ResolveSchema(ValidationRequest request, string schemaKey)
    {
        if (request.Schema != null)
        {
            return request.Schema;
        }

        if (!string.IsNullOrWhiteSpace(schemaKey) && request.Schemas.TryGetValue(schemaKey, out IStructureSchema schema))
        {
            return schema;
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

    private static string NormalizeAssertionPath(string path, string payloadAlias)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return string.Empty;
        }

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

        internal string PayloadAlias { get; set; } = "source";

        internal string SchemaKey { get; set; } = string.Empty;

        internal IReadOnlyCollection<IValidationRule> Rules { get; set; } = [];

        internal IReadOnlyCollection<IValidationAssertion> Assertions { get; set; } = [];
    }
}
