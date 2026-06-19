namespace ButterMorph.Functions;

using ButterMorph.Abstractions;

/// <summary>
/// Divides numeric values.
/// </summary>
public sealed class DivFunction : IFunction
{
    
    /// <summary>
    /// Gets the function description shown in design-time tooling.
    /// </summary>
    public string Description => "Divides numeric values.";

    // Shared conversion helpers for this function.
    private readonly FunctionTools _tools = new();

    /// <summary>
    /// Executes the function.
    /// </summary>
    /// <param name="context">The function execution context.</param>
    /// <returns>The function result.</returns>
    public IFunctionResult Execute(FunctionExecutionContext context)
    {
        _tools.Require(context, "div", 2, 16);
        double value = _tools.Number(context.Arguments[0]);

        for (int index = 1; index < context.Arguments.Count; index++)
        {
            value /= _tools.Number(context.Arguments[index]);
        }

        return _tools.NumberResult(value);
    }
}
