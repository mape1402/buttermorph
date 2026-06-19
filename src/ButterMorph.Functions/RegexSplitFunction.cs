namespace ButterMorph.Functions;

using System.Text.RegularExpressions;
using ButterMorph.Abstractions;

/// <summary>
/// Splits text using a regular expression.
/// </summary>
public sealed class RegexSplitFunction : IFunction
{
    
    /// <summary>
    /// Gets the function description shown in design-time tooling.
    /// </summary>
    public string Description => "Splits text using a regular expression.";

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
        _tools.Require(context, "regexSplit", 2, 3);
        List<IScalarValue> values = [];

        foreach (string value in Regex.Split(_tools.Text(context.Arguments[0]), _tools.Text(context.Arguments[1]), _regex.Options(context, 2)))
        {
            values.Add(_tools.StringValue(value));
        }

        return _tools.ScalarCollectionResult(values);
    }
}
