using ButterMorph.Abstractions;

/// <summary>
/// Demo validation rule handler used by the playground capability catalog.
/// </summary>
public sealed class DemoValidationRuleHandler : IValidationRuleHandler
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
