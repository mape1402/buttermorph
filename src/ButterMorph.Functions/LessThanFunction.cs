namespace ButterMorph.Functions;

using ButterMorph.Abstractions;

/// <summary>
/// Checks whether the first number is less than the second.
/// </summary>
public sealed class LessThanFunction : IFunction
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
        _tools.Require(context, "lt", 2, 2);
        return _tools.BooleanResult(_tools.Number(_tools.Argument(context, "lt", 0)) < _tools.Number(_tools.Argument(context, "lt", 1)));
    }
}
