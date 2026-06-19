namespace ButterMorph.Functions;

using ButterMorph.Abstractions;
using ButterMorph.Core;

/// <summary>
/// Sums numeric scalar collection values.
/// </summary>
public sealed class SumFunction : IFunction
{
    
    /// <summary>
    /// Gets the function description shown in design-time tooling.
    /// </summary>
    public string Description => "Sums numeric scalar collection values.";

    // Shared conversion helpers for this function.
    private readonly FunctionTools _tools = new();

    /// <summary>
    /// Executes the function.
    /// </summary>
    /// <param name="context">The function execution context.</param>
    /// <returns>The function result.</returns>
    public IFunctionResult Execute(FunctionExecutionContext context)
    {
        _tools.Require(context, "sum", 1, 1);
        double sum = 0;

        foreach (IScalarValue value in _tools.ScalarValues(context.Arguments[0]))
        {
            sum += _tools.Number(new ScalarFunctionArgument
            {
                Value = value
            });
        }

        return _tools.NumberResult(sum);
    }
}
