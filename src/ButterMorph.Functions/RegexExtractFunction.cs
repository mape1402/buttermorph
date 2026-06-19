namespace ButterMorph.Functions;

using System.Text.RegularExpressions;
using ButterMorph.Abstractions;

/// <summary>
/// Extracts a regular expression match or group.
/// </summary>
public sealed class RegexExtractFunction : IFunction
{
    
    /// <summary>
    /// Gets the function description shown in design-time tooling.
    /// </summary>
    public string Description => "Extracts a regular expression match or group.";

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
        _tools.Require(context, "regexExtract", 2, 4);
        Match match = Regex.Match(_tools.Text(context.Arguments[0]), _tools.Text(context.Arguments[1]), _regex.Options(context, 3));

        if (!match.Success)
        {
            return _tools.StringResult(string.Empty);
        }

        int groupIndex = 0;

        if (context.Arguments.Count > 2)
        {
            groupIndex = Convert.ToInt32(_tools.Number(context.Arguments[2]));
        }

        if (groupIndex >= 0 && groupIndex < match.Groups.Count)
        {
            return _tools.StringResult(match.Groups[groupIndex].Value);
        }

        return _tools.StringResult(string.Empty);
    }
}
