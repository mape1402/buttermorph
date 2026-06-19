namespace ButterMorph.Functions;

using ButterMorph.Abstractions;

/// <summary>
/// Returns true when any argument is truthy.
/// </summary>
public sealed class OrFunction : IFunction
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
        _tools.Require(context, "or", 0, 16);

        foreach (IFunctionArgument argument in context.Arguments)
        {
            if (_tools.Truthy(argument))
            {
                return _tools.BooleanResult(true);
            }
        }

        return _tools.BooleanResult(false);
    }
}
