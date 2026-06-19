namespace ButterMorph.Functions;

using ButterMorph.Abstractions;
using ButterMorph.Core;

/// <summary>
/// Returns whether a scalar collection contains a value.
/// </summary>
public sealed class ContainsValueFunction : IFunction
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
        _tools.Require(context, "containsValue", 2, 2);

        foreach (IScalarValue value in _tools.ScalarValues(context.Arguments[0]))
        {
            if (_tools.Same(new ScalarFunctionArgument
            {
                Value = value
            }, context.Arguments[1]))
            {
                return _tools.BooleanResult(true);
            }
        }

        return _tools.BooleanResult(false);
    }
}
