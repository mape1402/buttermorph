namespace ButterMorph.Functions;

using ButterMorph.Abstractions;

/// <summary>
/// Rounds a numeric value up.
/// </summary>
public sealed class CeilFunction : IFunction
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
        _tools.Require(context, "ceil", 1, 1);
        return _tools.NumberResult(Math.Ceiling(_tools.Number(context.Arguments[0])));
    }
}
