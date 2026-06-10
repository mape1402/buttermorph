namespace ButterMorph.UnitTests;

using ButterMorph.Abstractions;

/// <summary>
/// Test validation handler that accepts a rule without diagnostics.
/// </summary>
internal sealed class PassingValidationRuleHandler : IValidationRuleHandler
{
    /// <summary>
    /// Validates a rule context.
    /// </summary>
    /// <param name="context">The validation rule context.</param>
    /// <returns>The diagnostics produced by the handler.</returns>
    public IReadOnlyCollection<DiagnosticEntry> Validate(ValidationRuleContext context)
    {
        return [];
    }
}
