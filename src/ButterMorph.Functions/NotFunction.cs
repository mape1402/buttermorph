namespace ButterMorph.Functions;

using ButterMorph.Abstractions;

/// <summary>
/// Negates a truthy value.
/// </summary>
public sealed class NotFunction : IFunction
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
        _tools.Require(context, "not", 1, 1);
        return _tools.BooleanResult(!_tools.Truthy(_tools.Argument(context, "not", 0)));
    }
}
