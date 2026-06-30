namespace ButterMorph.Functions;

using System.Text.RegularExpressions;
using ButterMorph.Abstractions;

/// <summary>
/// Replaces regular expression matches.
/// </summary>
public sealed class RegexReplaceFunction : IFunction
{
    
    /// <summary>
    /// Gets the function description shown in design-time tooling.
    /// </summary>
    public string Description => "Replaces regular expression matches.";

    // Shared conversion helpers for this function.
    private readonly FunctionTools _tools = new();

    // Shared regex option helpers for this function.
    private readonly RegexFunctionTools _regex = new();

    /// <summary>
    /// Executes the function.
    /// </summary>
    /// <param name="context">The function execution context.</param>
    /// <returns>The function result.</returns>
    public IFunctionResult Execute(FunctionExecutionContext context)
    {
        _tools.Require(context, "regexReplace", 3, 4);
        return _tools.StringResult(Regex.Replace(_tools.Text(context.Arguments[0]), _tools.Text(context.Arguments[1]), _tools.Text(context.Arguments[2]), _regex.Options(context, 3)));
    }
}
