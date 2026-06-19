namespace ButterMorph.Functions;

using ButterMorph.Abstractions;

/// <summary>
/// Returns a fallback when a value is null.
/// </summary>
public sealed class DefaultFunction : IFunction
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
        _tools.Require(context, "default", 2, 2);
        IFunctionArgument value = _tools.Argument(context, "default", 0);

        if (_tools.IsNull(value))
        {
            return _tools.CloneArgument(_tools.Argument(context, "default", 1));
        }

        return _tools.CloneArgument(value);
    }
}
