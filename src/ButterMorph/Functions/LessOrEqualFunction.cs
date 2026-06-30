namespace ButterMorph.Functions;

using ButterMorph.Abstractions;

/// <summary>
/// Checks whether the first number is less than or equal to the second.
/// </summary>
public sealed class LessOrEqualFunction : IFunction
{
    
    /// <summary>
    /// Gets the function description shown in design-time tooling.
    /// </summary>
    public string Description => "Checks whether the first number is less than or equal to the second.";

    // Shared conversion helpers for this function.
    private readonly FunctionTools _tools = new();

    /// <summary>
    /// Executes the function.
    /// </summary>
    /// <param name="context">The function execution context.</param>
    /// <returns>The function result.</returns>
    public IFunctionResult Execute(FunctionExecutionContext context)
    {
        _tools.Require(context, "lte", 2, 2);
        return _tools.BooleanResult(_tools.Number(_tools.Argument(context, "lte", 0)) <= _tools.Number(_tools.Argument(context, "lte", 1)));
    }
}
