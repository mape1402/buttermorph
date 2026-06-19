namespace ButterMorph.Functions;

using ButterMorph.Abstractions;

/// <summary>
/// Checks whether a value is null.
/// </summary>
public sealed class IsNullFunction : IFunction
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
        _tools.Require(context, "isNull", 1, 1);
        return _tools.BooleanResult(_tools.IsNull(_tools.Argument(context, "isNull", 0)));
    }
}
