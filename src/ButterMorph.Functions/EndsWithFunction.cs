namespace ButterMorph.Functions;

using ButterMorph.Abstractions;

/// <summary>
/// Checks whether text ends with a suffix.
/// </summary>
public sealed class EndsWithFunction : IFunction
{
    
    /// <summary>
    /// Gets the function description shown in design-time tooling.
    /// </summary>
    public string Description => "Checks whether text ends with a suffix.";

    // Shared conversion helpers for this function.
    private readonly FunctionTools _tools = new();

    /// <summary>
    /// Executes the function.
    /// </summary>
    /// <param name="context">The function execution context.</param>
    /// <returns>The function result.</returns>
    public IFunctionResult Execute(FunctionExecutionContext context)
    {
        _tools.Require(context, "endsWith", 2, 2);
        string text = _tools.Text(_tools.Argument(context, "endsWith", 0));
        string suffix = _tools.Text(_tools.Argument(context, "endsWith", 1));
        return _tools.BooleanResult(text.EndsWith(suffix, StringComparison.Ordinal));
    }
}
