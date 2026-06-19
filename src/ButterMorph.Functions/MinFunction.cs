namespace ButterMorph.Functions;

using ButterMorph.Abstractions;

/// <summary>
/// Returns the minimum numeric value.
/// </summary>
public sealed class MinFunction : IFunction
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
        _tools.Require(context, "min", 1, 16);
        double value = _tools.Number(context.Arguments[0]);

        foreach (IFunctionArgument argument in context.Arguments)
        {
            value = Math.Min(value, _tools.Number(argument));
        }

        return _tools.NumberResult(value);
    }
}
