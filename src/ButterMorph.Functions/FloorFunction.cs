namespace ButterMorph.Functions;

using ButterMorph.Abstractions;

/// <summary>
/// Rounds a numeric value down.
/// </summary>
public sealed class FloorFunction : IFunction
{
    
    /// <summary>
    /// Gets the function description shown in design-time tooling.
    /// </summary>
    public string Description => "Rounds a numeric value down.";

    // Shared conversion helpers for this function.
    private readonly FunctionTools _tools = new();

    /// <summary>
    /// Executes the function.
    /// </summary>
    /// <param name="context">The function execution context.</param>
    /// <returns>The function result.</returns>
    public IFunctionResult Execute(FunctionExecutionContext context)
    {
        _tools.Require(context, "floor", 1, 1);
        return _tools.NumberResult(Math.Floor(_tools.Number(context.Arguments[0])));
    }
}
