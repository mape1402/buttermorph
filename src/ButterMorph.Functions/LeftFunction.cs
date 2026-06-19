namespace ButterMorph.Functions;

using ButterMorph.Abstractions;

/// <summary>
/// Returns the left part of text.
/// </summary>
public sealed class LeftFunction : IFunction
{
    // Shared conversion helpers for this function.
    private readonly FunctionTools _tools = new();

    /// <summary>
    /// Executes the function.
    /// </summary>
    /// <param name="context">The function execution context.</param>
    /// <returns>The function result.</returns>
    public IFunctionResult Execute(FunctionExecutionContext context)
    {
        _tools.Require(context, "left", 2, 2);
        string text = _tools.Text(context.Arguments[0]);
        int count = Math.Max(0, Convert.ToInt32(_tools.Number(context.Arguments[1])));

        if (count >= text.Length)
        {
            return _tools.StringResult(text);
        }

        return _tools.StringResult(text[..count]);
    }
}
