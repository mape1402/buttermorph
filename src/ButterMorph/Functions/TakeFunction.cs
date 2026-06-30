namespace ButterMorph.Functions;

using ButterMorph.Abstractions;

/// <summary>
/// Takes the first scalar collection values.
/// </summary>
public sealed class TakeFunction : IFunction
{
    
    /// <summary>
    /// Gets the function description shown in design-time tooling.
    /// </summary>
    public string Description => "Takes the first scalar collection values.";

    // Shared conversion helpers for this function.
    private readonly FunctionTools _tools = new();

    /// <summary>
    /// Executes the function.
    /// </summary>
    /// <param name="context">The function execution context.</param>
    /// <returns>The function result.</returns>
    public IFunctionResult Execute(FunctionExecutionContext context)
    {
        _tools.Require(context, "take", 2, 2);
        int count = Math.Max(0, Convert.ToInt32(_tools.Number(context.Arguments[1])));
        return _tools.ScalarCollectionResult(_tools.ScalarValues(context.Arguments[0]).Take(count).ToList());
    }
}
