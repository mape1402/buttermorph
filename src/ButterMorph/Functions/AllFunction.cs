namespace ButterMorph.Functions;

using ButterMorph.Abstractions;

/// <summary>
/// Returns whether all scalar collection values are truthy.
/// </summary>
public sealed class AllFunction : IFunction
{
    
    /// <summary>
    /// Gets the function description shown in design-time tooling.
    /// </summary>
    public string Description => "Returns whether all scalar collection values are truthy.";

    // Shared conversion helpers for this function.
    private readonly FunctionTools _tools = new();

    /// <summary>
    /// Executes the function.
    /// </summary>
    /// <param name="context">The function execution context.</param>
    /// <returns>The function result.</returns>
    public IFunctionResult Execute(FunctionExecutionContext context)
    {
        _tools.Require(context, "all", 1, 1);
        IReadOnlyCollection<IScalarValue> values = _tools.ScalarValues(context.Arguments[0]);

        if (values.Count == 0)
        {
            return _tools.BooleanResult(false);
        }

        foreach (IScalarValue value in values)
        {
            if (!_tools.Truthy(value))
            {
                return _tools.BooleanResult(false);
            }
        }

        return _tools.BooleanResult(true);
    }
}
