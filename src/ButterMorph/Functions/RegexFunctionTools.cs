namespace ButterMorph.Functions;

using System.Text.RegularExpressions;
using ButterMorph.Abstractions;

internal sealed class RegexFunctionTools
{
    // Shared scalar conversion helper.
    private readonly FunctionTools _tools = new();

    internal RegexOptions Options(FunctionExecutionContext context, int index)
    {
        if (context.Arguments.Count <= index)
        {
            return RegexOptions.None;
        }

        string text = _tools.Text(context.Arguments[index]);
        RegexOptions options = RegexOptions.None;

        if (text.Contains("ignoreCase", StringComparison.OrdinalIgnoreCase) || text.Contains("caseInsensitive", StringComparison.OrdinalIgnoreCase))
        {
            options |= RegexOptions.IgnoreCase;
        }

        if (text.Contains("multiline", StringComparison.OrdinalIgnoreCase))
        {
            options |= RegexOptions.Multiline;
        }

        return options;
    }
}
