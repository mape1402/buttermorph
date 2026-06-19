namespace ButterMorph.Functions;

using ButterMorph.Abstractions;

/// <summary>
/// Checks whether the first number is greater than or equal to the second.
/// </summary>
public sealed class GreaterOrEqualFunction : IFunction
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
        _tools.Require(context, "gte", 2, 2);
        return _tools.BooleanResult(_tools.Number(_tools.Argument(context, "gte", 0)) >= _tools.Number(_tools.Argument(context, "gte", 1)));
    }
}
