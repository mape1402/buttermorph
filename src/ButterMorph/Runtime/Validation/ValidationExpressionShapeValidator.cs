namespace ButterMorph.Validation;

using ButterMorph.Abstractions;

/// <summary>
/// Validates the structural rules for validation-specific expressions.
/// </summary>
public static class ValidationExpressionShapeValidator
{
    /// <summary>
    /// Validates every assertion in a validation document.
    /// </summary>
    /// <param name="document">The validation document.</param>
    /// <returns>The shape diagnostics.</returns>
    public static IReadOnlyCollection<DiagnosticEntry> ValidateDocument(IValidationDocument document)
    {
        if (document == null)
        {
            return [];
        }

        return ValidateStatements(document.Statements);
    }

    /// <summary>
    /// Validates every assertion in a statement list.
    /// </summary>
    /// <param name="statements">The validation statements.</param>
    /// <returns>The shape diagnostics.</returns>
    public static IReadOnlyCollection<DiagnosticEntry> ValidateStatements(IReadOnlyCollection<IValidationStatement> statements)
    {
        List<DiagnosticEntry> diagnostics = [];

        if (statements == null)
        {
            return diagnostics;
        }

        foreach (IValidationStatement statement in statements)
        {
            ValidateStatement(statement, diagnostics);
        }

        return diagnostics;
    }

    /// <summary>
    /// Validates one assertion expression.
    /// </summary>
    /// <param name="assertion">The validation assertion.</param>
    /// <param name="path">The diagnostic path override.</param>
    /// <returns>The shape diagnostics.</returns>
    public static IReadOnlyCollection<DiagnosticEntry> ValidateAssertion(IValidationAssertion assertion, string path = null)
    {
        List<DiagnosticEntry> diagnostics = [];

        if (assertion == null)
        {
            return diagnostics;
        }

        ValidateRoot(assertion.Expression, string.IsNullOrWhiteSpace(path) ? assertion.Path : path, diagnostics);
        return diagnostics;
    }

    /// <summary>
    /// Validates one root validation expression.
    /// </summary>
    /// <param name="expression">The validation expression.</param>
    /// <param name="path">The diagnostic path.</param>
    /// <returns>The shape diagnostics.</returns>
    public static IReadOnlyCollection<DiagnosticEntry> ValidateExpression(ITransformationExpression expression, string path = null)
    {
        List<DiagnosticEntry> diagnostics = [];
        ValidateRoot(expression, path ?? string.Empty, diagnostics);
        return diagnostics;
    }

    private static void ValidateStatement(IValidationStatement statement, List<DiagnosticEntry> diagnostics)
    {
        if (statement is IValidationAssertion assertion)
        {
            ValidateRoot(assertion.Expression, assertion.Path, diagnostics);
            return;
        }

        if (statement is IValidationForEach forEach)
        {
            foreach (IValidationStatement childStatement in forEach.Statements)
            {
                ValidateStatement(childStatement, diagnostics);
            }
        }
    }

    private static void ValidateRoot(ITransformationExpression expression, string path, List<DiagnosticEntry> diagnostics)
    {
        ValidateExpression(expression, true, ConditionalPlacement.Root, path ?? string.Empty, diagnostics);
    }

    private static void ValidateExpression(
        ITransformationExpression expression,
        bool allowConditional,
        ConditionalPlacement placement,
        string path,
        List<DiagnosticEntry> diagnostics)
    {
        if (expression == null)
        {
            diagnostics.Add(CreateDiagnostic("BMVL010", "Validation expression is required.", path));
            return;
        }

        if (expression is IConditionalExpression conditional)
        {
            if (!allowConditional)
            {
                diagnostics.Add(CreateDiagnostic("BMVL010", ResolveConditionalMessage(placement), path));
                return;
            }

            ValidateExpression(conditional.Condition, false, ConditionalPlacement.WhenCondition, path, diagnostics);
            ValidateExpression(conditional.ThenExpression, true, ConditionalPlacement.WhenBranch, path, diagnostics);

            if (conditional.ElseExpression != null)
            {
                ValidateExpression(conditional.ElseExpression, true, ConditionalPlacement.WhenBranch, path, diagnostics);
            }

            return;
        }

        if (expression is IFunctionCallExpression function)
        {
            bool isGroup = IsLogicalGroup(function.FunctionKey);
            ConditionalPlacement childPlacement = isGroup ? ConditionalPlacement.Group : ConditionalPlacement.FunctionArgument;

            foreach (ITransformationExpression argument in function.Arguments)
            {
                ValidateExpression(argument, false, childPlacement, path, diagnostics);
            }

            return;
        }

        if (expression is IObjectExpression map)
        {
            foreach (IObjectPropertyExpression property in map.Properties)
            {
                ValidateExpression(property.Expression, allowConditional, placement, path, diagnostics);
            }

            return;
        }

        if (expression is IArrayExpression ordered)
        {
            foreach (ITransformationExpression item in ordered.Items)
            {
                ValidateExpression(item, allowConditional, placement, path, diagnostics);
            }

            return;
        }

        if (expression is ICollectionProjectionExpression projection)
        {
            ValidateExpression(projection.SourceExpression, false, ConditionalPlacement.FunctionArgument, path, diagnostics);
            ValidateExpression(projection.BodyExpression, allowConditional, placement, path, diagnostics);
        }
    }

    private static bool IsLogicalGroup(string functionKey)
    {
        return string.Equals(functionKey, "and", StringComparison.Ordinal) ||
            string.Equals(functionKey, "or", StringComparison.Ordinal);
    }

    private static string ResolveConditionalMessage(ConditionalPlacement placement)
    {
        return placement switch
        {
            ConditionalPlacement.WhenCondition => "The condition of a when cannot contain another when.",
            ConditionalPlacement.Group => "Validation groups cannot contain when. Put the when as the whole rule, then branch, or otherwise branch.",
            ConditionalPlacement.FunctionArgument => "Function arguments cannot contain validation when expressions.",
            _ => "when can only be used as a complete validation rule or as the complete then/otherwise branch of another when."
        };
    }

    private static DiagnosticEntry CreateDiagnostic(string code, string message, string path)
    {
        return new DiagnosticEntry
        {
            Code = code,
            Message = message,
            Path = path ?? string.Empty,
            Severity = "Error"
        };
    }

    private enum ConditionalPlacement
    {
        Root,
        WhenCondition,
        WhenBranch,
        Group,
        FunctionArgument
    }
}
