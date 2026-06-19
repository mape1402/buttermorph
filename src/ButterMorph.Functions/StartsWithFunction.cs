namespace ButterMorph.Functions;

using ButterMorph.Abstractions;

/// <summary>
/// Checks whether text starts with a prefix.
/// </summary>
public sealed class StartsWithFunction : IFunction
{
    
    /// <summary>
    /// Gets the function description shown in design-time tooling.
    /// </summary>
    public string Description => "Checks whether text starts with a prefix.";

    // Shared conversion helpers for this function.
    private readonly FunctionTools _tools = new();

    /// <summary>
    /// Executes the function.
    /// </summary>
    /// <param name="context">The function execution context.</param>
    /// <returns>The function result.</returns>
    public IFunctionResult Execute(FunctionExecutionContext context)
    {
        _tools.Require(context, "startsWith", 2, 2);
        string text = _tools.Text(_tools.Argument(context, "startsWith", 0));
        string prefix = _tools.Text(_tools.Argument(context, "startsWith", 1));
        return _tools.BooleanResult(text.StartsWith(prefix, StringComparison.Ordinal));
    }
}
