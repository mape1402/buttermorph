namespace ButterMorph.Functions;

using ButterMorph.Abstractions;

/// <summary>
/// Multiplies numeric values.
/// </summary>
public sealed class MulFunction : IFunction
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
        _tools.Require(context, "mul", 1, 16);
        double value = 1d;

        foreach (IFunctionArgument argument in context.Arguments)
        {
            value *= _tools.Number(argument);
        }

        return _tools.NumberResult(value);
    }
}
