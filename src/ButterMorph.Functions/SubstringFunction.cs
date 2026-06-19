namespace ButterMorph.Functions;

using ButterMorph.Abstractions;

/// <summary>
/// Extracts a substring from text.
/// </summary>
public sealed class SubstringFunction : IFunction
{
    
    /// <summary>
    /// Gets the function description shown in design-time tooling.
    /// </summary>
    public string Description => "Extracts a substring from text.";

    // Shared conversion helpers for this function.
    private readonly FunctionTools _tools = new();

    /// <summary>
    /// Executes the function.
    /// </summary>
    /// <param name="context">The function execution context.</param>
    /// <returns>The function result.</returns>
    public IFunctionResult Execute(FunctionExecutionContext context)
    {
        _tools.Require(context, "substring", 2, 3);
        string text = _tools.Text(_tools.Argument(context, "substring", 0));
        int start = Math.Max(0, Convert.ToInt32(_tools.Number(_tools.Argument(context, "substring", 1))));

        if (start >= text.Length)
        {
            return _tools.StringResult(string.Empty);
        }

        if (context.Arguments.Count == 2)
        {
            return _tools.StringResult(text[start..]);
        }

        int length = Math.Max(0, Convert.ToInt32(_tools.Number(_tools.Argument(context, "substring", 2))));
        length = Math.Min(length, text.Length - start);
        return _tools.StringResult(text.Substring(start, length));
    }
}
