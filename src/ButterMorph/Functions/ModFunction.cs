namespace ButterMorph.Functions;

using ButterMorph.Abstractions;

/// <summary>
/// Returns the numeric remainder.
/// </summary>
public sealed class ModFunction : IFunction
{
    
    /// <summary>
    /// Gets the function description shown in design-time tooling.
    /// </summary>
    public string Description => "Returns the numeric remainder.";

    // Shared conversion helpers for this function.
    private readonly FunctionTools _tools = new();

    /// <summary>
    /// Executes the function.
    /// </summary>
    /// <param name="context">The function execution context.</param>
    /// <returns>The function result.</returns>
    public IFunctionResult Execute(FunctionExecutionContext context)
    {
        _tools.Require(context, "mod", 2, 2);
        return _tools.NumberResult(_tools.Number(context.Arguments[0]) % _tools.Number(context.Arguments[1]));
    }
}
