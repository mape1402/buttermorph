namespace ButterMorph.Functions;

using ButterMorph.Abstractions;

/// <summary>
/// Checks whether two values are equal.
/// </summary>
public sealed class EqualToFunction : IFunction
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
        _tools.Require(context, "eq", 2, 2);
        return _tools.BooleanResult(_tools.Same(_tools.Argument(context, "eq", 0), _tools.Argument(context, "eq", 1)));
    }
}
