namespace ButterMorph.Functions;

using ButterMorph.Abstractions;

/// <summary>
/// Returns a fallback when the evaluated value is empty.
/// </summary>
public sealed class TryFunction : IFunction
{
    
    /// <summary>
    /// Gets the function description shown in design-time tooling.
    /// </summary>
    public string Description => "Returns a fallback when the evaluated value is empty.";

    // Shared conversion helpers for this function.
    private readonly FunctionTools _tools = new();

    /// <summary>
    /// Executes the function.
    /// </summary>
    /// <param name="context">The function execution context.</param>
    /// <returns>The function result.</returns>
    public IFunctionResult Execute(FunctionExecutionContext context)
    {
        _tools.Require(context, "try", 1, 2);
        IFunctionArgument value = _tools.Argument(context, "try", 0);

        if (_tools.IsEmpty(value) && context.Arguments.Count > 1)
        {
            return _tools.CloneArgument(_tools.Argument(context, "try", 1));
        }

        return _tools.CloneArgument(value);
    }
}
