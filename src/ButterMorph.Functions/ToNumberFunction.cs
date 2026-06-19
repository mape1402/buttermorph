namespace ButterMorph.Functions;

using ButterMorph.Abstractions;

/// <summary>
/// Converts a value to a number.
/// </summary>
public sealed class ToNumberFunction : IFunction
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
        _tools.Require(context, "toNumber", 1, 1);
        return _tools.NumberResult(_tools.Number(context.Arguments[0]));
    }
}
