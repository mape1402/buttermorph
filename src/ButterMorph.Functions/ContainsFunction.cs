namespace ButterMorph.Functions;

using ButterMorph.Abstractions;

/// <summary>
/// Checks whether text contains a value.
/// </summary>
public sealed class ContainsFunction : IFunction
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
        _tools.Require(context, "contains", 2, 2);
        string text = _tools.Text(_tools.Argument(context, "contains", 0));
        string value = _tools.Text(_tools.Argument(context, "contains", 1));
        return _tools.BooleanResult(text.Contains(value, StringComparison.Ordinal));
    }
}
