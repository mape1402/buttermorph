namespace ButterMorph.UnitTests;

using ButterMorph.Abstractions;

/// <summary>
/// Test validation handler that always returns one diagnostic.
/// </summary>
internal sealed class DiagnosticValidationRuleHandler : IValidationRuleHandler
{
    /// <summary>
    /// Validates a rule context.
    /// </summary>
    /// <param name="context">The validation rule context.</param>
    /// <returns>The diagnostics produced by the handler.</returns>
    public IReadOnlyCollection<DiagnosticEntry> Validate(ValidationRuleContext context)
    {
        return
        [
            new DiagnosticEntry
            {
                Code = "TEST001",
                Message = "The test handler rejected the value.",
                Path = context.Path,
                Severity = "Error"
            }
        ];
    }
}
