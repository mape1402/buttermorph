namespace ButterMorph.Functions;

using ButterMorph.Abstractions;

/// <summary>
/// Returns the absolute numeric value.
/// </summary>
public sealed class AbsFunction : IFunction
{
    
    /// <summary>
    /// Gets the function description shown in design-time tooling.
    /// </summary>
    public string Description => "Returns the absolute numeric value.";

    // Shared conversion helpers for this function.
    private readonly FunctionTools _tools = new();

    /// <summary>
    /// Executes the function.
    /// </summary>
    /// <param name="context">The function execution context.</param>
    /// <returns>The function result.</returns>
    public IFunctionResult Execute(FunctionExecutionContext context)
    {
        _tools.Require(context, "abs", 1, 1);
        return _tools.NumberResult(Math.Abs(_tools.Number(context.Arguments[0])));
    }
}
