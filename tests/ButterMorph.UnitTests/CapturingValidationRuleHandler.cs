namespace ButterMorph.UnitTests;

using ButterMorph.Abstractions;
using ButterMorph.Core;

/// <summary>
/// Test validation handler that captures the resolved context.
/// </summary>
internal sealed class CapturingValidationRuleHandler : IValidationRuleHandler
{
    /// <summary>
    /// Gets the node received during validation.
    /// </summary>
    public IStructureNode CapturedNode { get; private set; } = new StructureNode();

    /// <summary>
    /// Gets the path received during validation.
    /// </summary>
    public string CapturedPath { get; private set; } = string.Empty;

    /// <summary>
    /// Validates a rule context.
    /// </summary>
    /// <param name="context">The validation rule context.</param>
    /// <returns>The diagnostics produced by the handler.</returns>
    public IReadOnlyCollection<DiagnosticEntry> Validate(ValidationRuleContext context)
    {
        CapturedNode = context.Node;
        CapturedPath = context.Path;
        return [];
    }
}
