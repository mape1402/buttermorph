namespace ButterMorph.Functions;

using ButterMorph.Abstractions;

/// <summary>
/// Returns the right part of text.
/// </summary>
public sealed class RightFunction : IFunction
{
    
    /// <summary>
    /// Gets the function description shown in design-time tooling.
    /// </summary>
    public string Description => "Returns the right part of text.";

    // Shared conversion helpers for this function.
    private readonly FunctionTools _tools = new();

    /// <summary>
    /// Executes the function.
    /// </summary>
    /// <param name="context">The function execution context.</param>
    /// <returns>The function result.</returns>
    public IFunctionResult Execute(FunctionExecutionContext context)
    {
        _tools.Require(context, "right", 2, 2);
        string text = _tools.Text(context.Arguments[0]);
        int count = Math.Max(0, Convert.ToInt32(_tools.Number(context.Arguments[1])));

        if (count >= text.Length)
        {
            return _tools.StringResult(text);
        }

        return _tools.StringResult(text[^count..]);
    }
}
