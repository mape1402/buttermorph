namespace ButterMorph.Functions;

using ButterMorph.Abstractions;

/// <summary>
/// Returns whether any scalar collection value is truthy.
/// </summary>
public sealed class AnyFunction : IFunction
{
    
    /// <summary>
    /// Gets the function description shown in design-time tooling.
    /// </summary>
    public string Description => "Returns whether any scalar collection value is truthy.";

    // Shared conversion helpers for this function.
    private readonly FunctionTools _tools = new();

    /// <summary>
    /// Executes the function.
    /// </summary>
    /// <param name="context">The function execution context.</param>
    /// <returns>The function result.</returns>
    public IFunctionResult Execute(FunctionExecutionContext context)
    {
        _tools.Require(context, "any", 1, 1);

        foreach (IScalarValue value in _tools.ScalarValues(context.Arguments[0]))
        {
            if (_tools.Truthy(value))
            {
                return _tools.BooleanResult(true);
            }
        }

        return _tools.BooleanResult(false);
    }
}
