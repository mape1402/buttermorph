namespace ButterMorph.Functions;

using ButterMorph.Abstractions;

/// <summary>
/// Converts a value to a boolean scalar.
/// </summary>
public sealed class ToBooleanFunction : IFunction
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
        _tools.Require(context, "toBoolean", 1, 1);
        return _tools.BooleanResult(_tools.Truthy(context.Arguments[0]));
    }
}
