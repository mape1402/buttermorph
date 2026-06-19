namespace ButterMorph.Functions;

using ButterMorph.Abstractions;

/// <summary>
/// Adds numeric values.
/// </summary>
public sealed class AddFunction : IFunction
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
        _tools.Require(context, "add", 1, 16);
        double total = 0d;

        foreach (IFunctionArgument argument in context.Arguments)
        {
            total += _tools.Number(argument);
        }

        return _tools.NumberResult(total);
    }
}
