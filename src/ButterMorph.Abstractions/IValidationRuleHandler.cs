namespace ButterMorph.Abstractions;

using System.Collections.Generic;

/// <summary>
/// Defines executable behavior for a validation rule.
/// </summary>
public interface IValidationRuleHandler
{
    /// <summary>
    /// Validates a rule context.
    /// </summary>
    /// <param name="context">The validation rule context.</param>
    /// <returns>The diagnostics produced by the handler.</returns>
    IReadOnlyCollection<DiagnosticEntry> Validate(ValidationRuleContext context);
}
