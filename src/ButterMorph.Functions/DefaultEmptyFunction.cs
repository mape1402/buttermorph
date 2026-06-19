namespace ButterMorph.Functions;

using ButterMorph.Abstractions;

/// <summary>
/// Returns a fallback when a value is null or empty.
/// </summary>
public sealed class DefaultEmptyFunction : IFunction
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
        _tools.Require(context, "defaultEmpty", 2, 2);
        IFunctionArgument value = _tools.Argument(context, "defaultEmpty", 0);

        if (_tools.IsEmpty(value))
        {
            return _tools.CloneArgument(_tools.Argument(context, "defaultEmpty", 1));
        }

        return _tools.CloneArgument(value);
    }
}
