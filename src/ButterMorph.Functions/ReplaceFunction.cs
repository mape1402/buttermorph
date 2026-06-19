namespace ButterMorph.Functions;

using ButterMorph.Abstractions;

/// <summary>
/// Replaces text using ordinal comparison.
/// </summary>
public sealed class ReplaceFunction : IFunction
{
    
    /// <summary>
    /// Gets the function description shown in design-time tooling.
    /// </summary>
    public string Description => "Replaces text using ordinal comparison.";

    // Shared conversion helpers for this function.
    private readonly FunctionTools _tools = new();

    /// <summary>
    /// Executes the function.
    /// </summary>
    /// <param name="context">The function execution context.</param>
    /// <returns>The function result.</returns>
    public IFunctionResult Execute(FunctionExecutionContext context)
    {
        _tools.Require(context, "replace", 3, 3);
        string text = _tools.Text(_tools.Argument(context, "replace", 0));
        string oldValue = _tools.Text(_tools.Argument(context, "replace", 1));
        string newValue = _tools.Text(_tools.Argument(context, "replace", 2));
        return _tools.StringResult(text.Replace(oldValue, newValue, StringComparison.Ordinal));
    }
}
