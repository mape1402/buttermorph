namespace ButterMorph.Functions;

using System.Text.RegularExpressions;
using ButterMorph.Abstractions;

/// <summary>
/// Normalizes whitespace in text.
/// </summary>
public sealed class NormalizeWhitespaceFunction : IFunction
{
    
    /// <summary>
    /// Gets the function description shown in design-time tooling.
    /// </summary>
    public string Description => "Normalizes whitespace in text.";

    // Shared conversion helpers for this function.
    private readonly FunctionTools _tools = new();

    /// <summary>
    /// Executes the function.
    /// </summary>
    /// <param name="context">The function execution context.</param>
    /// <returns>The function result.</returns>
    public IFunctionResult Execute(FunctionExecutionContext context)
    {
        _tools.Require(context, "normalizeWhitespace", 1, 1);
        return _tools.StringResult(Regex.Replace(_tools.Text(context.Arguments[0]), "\\s+", " ").Trim());
    }
}
