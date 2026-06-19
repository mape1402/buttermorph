namespace ButterMorph.Functions;

using ButterMorph.Abstractions;

/// <summary>
/// Returns a slice of scalar collection values.
/// </summary>
public sealed class SliceFunction : IFunction
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
        _tools.Require(context, "slice", 3, 3);
        int start = Math.Max(0, Convert.ToInt32(_tools.Number(context.Arguments[1])));
        int count = Math.Max(0, Convert.ToInt32(_tools.Number(context.Arguments[2])));
        return _tools.ScalarCollectionResult(_tools.ScalarValues(context.Arguments[0]).Skip(start).Take(count).ToList());
    }
}
