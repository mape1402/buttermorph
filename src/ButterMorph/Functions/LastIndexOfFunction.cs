namespace ButterMorph.Functions;

using ButterMorph.Abstractions;

/// <summary>
/// Returns the last index of a search value.
/// </summary>
public sealed class LastIndexOfFunction : IFunction
{
    
    /// <summary>
    /// Gets the function description shown in design-time tooling.
    /// </summary>
    public string Description => "Returns the last index of a search value.";

    // Shared conversion helpers for this function.
    private readonly FunctionTools _tools = new();

    /// <summary>
    /// Executes the function.
    /// </summary>
    /// <param name="context">The function execution context.</param>
    /// <returns>The function result.</returns>
    public IFunctionResult Execute(FunctionExecutionContext context)
    {
        _tools.Require(context, "lastIndexOf", 2, 2);
        return _tools.NumberResult(_tools.Text(context.Arguments[0]).LastIndexOf(_tools.Text(context.Arguments[1]), StringComparison.Ordinal));
    }
}
