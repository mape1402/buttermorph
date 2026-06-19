namespace ButterMorph.Functions;

using System.Text.RegularExpressions;
using ButterMorph.Abstractions;

/// <summary>
/// Finds all regular expression matches.
/// </summary>
public sealed class RegexFindAllFunction : IFunction
{
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
        _tools.Require(context, "regexFindAll", 2, 4);
        int groupIndex = 0;

        if (context.Arguments.Count > 2)
        {
            groupIndex = Convert.ToInt32(_tools.Number(context.Arguments[2]));
        }

        List<IScalarValue> values = [];

        foreach (Match match in Regex.Matches(_tools.Text(context.Arguments[0]), _tools.Text(context.Arguments[1]), _regex.Options(context, 3)))
        {
            if (groupIndex >= 0 && groupIndex < match.Groups.Count)
            {
                values.Add(_tools.StringValue(match.Groups[groupIndex].Value));
            }
        }

        return _tools.ScalarCollectionResult(values);
    }
}
