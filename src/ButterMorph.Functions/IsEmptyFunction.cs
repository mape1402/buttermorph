namespace ButterMorph.Functions;

using ButterMorph.Abstractions;

/// <summary>
/// Checks whether a value is empty.
/// </summary>
public sealed class IsEmptyFunction : IFunction
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
        _tools.Require(context, "isEmpty", 1, 1);
        return _tools.BooleanResult(_tools.IsEmpty(_tools.Argument(context, "isEmpty", 0)));
    }
}
