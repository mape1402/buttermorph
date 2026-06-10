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

    /// <summary>
    /// Initializes a new instance of the <see cref="ValidationEngine"/> class.
    /// </summary>
    /// <param name="pathResolver">The path resolver.</param>
    /// <param name="ruleRegistry">The validation rule registry.</param>
    public ValidationEngine(IPathResolver pathResolver, IValidationRuleRegistry ruleRegistry)
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
    }

    /// <summary>
    /// Executes a validation request.
    /// </summary>
    /// <param name="request">The validation request.</param>
    /// <returns>The validation result.</returns>
    public ValidationResult Validate(ValidationRequest request)
    {
        List<DiagnosticEntry> diagnostics = [];

        if (request.Definition is not IValidationDocument document)
        {
            diagnostics.Add(CreateDiagnostic("BMVL001", "Validation request definition must implement IValidationDocument.", string.Empty));
            return CreateResult(diagnostics);
        }

        foreach (IValidationRule rule in document.Rules)
        {
            ValidateRule(request.SourceGraph.Root, rule, diagnostics);
        }

        return CreateResult(diagnostics);
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
}
